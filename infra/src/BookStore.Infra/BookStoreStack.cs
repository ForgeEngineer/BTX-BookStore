using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using Amazon.CDK.AWS.SecretsManager;
using Constructs;
using EcsSecret = Amazon.CDK.AWS.ECS.Secret;
using ElbHealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;
using Distribution = Amazon.CDK.AWS.CloudFront.Distribution;
using Function = Amazon.CDK.AWS.CloudFront.Function;
using FunctionProps = Amazon.CDK.AWS.CloudFront.FunctionProps;
using ImagePlatform = Amazon.CDK.AWS.Ecr.Assets.Platform_;
using SecretsManagerSecret = Amazon.CDK.AWS.SecretsManager.Secret;

namespace BookStore.Infra;

public sealed class BookStoreStack : Stack
{
    private const string CloudFrontOriginFacingPrefixList = "pl-3b927c52";
    private const string DatabaseName = "bookstore";
    private const string DatabaseUser = "bookstore";

    public BookStoreStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
    {
        var root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".."));

        var vpc = new Vpc(this, "Vpc", new VpcProps
        {
            MaxAzs = 2,
            NatGateways = 0,
            SubnetConfiguration =
            [
                new SubnetConfiguration { Name = "public", SubnetType = SubnetType.PUBLIC, CidrMask = 24 },
                new SubnetConfiguration { Name = "isolated", SubnetType = SubnetType.PRIVATE_ISOLATED, CidrMask = 24 }
            ]
        });

        var database = new DatabaseInstance(this, "Database", new DatabaseInstanceProps
        {
            Engine = DatabaseInstanceEngine.Postgres(new PostgresInstanceEngineProps { Version = PostgresEngineVersion.VER_17 }),
            InstanceType = Amazon.CDK.AWS.EC2.InstanceType.Of(InstanceClass.T4G, InstanceSize.MICRO),
            Vpc = vpc,
            VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE_ISOLATED },
            Credentials = Credentials.FromGeneratedSecret(DatabaseUser),
            DatabaseName = DatabaseName,
            AllocatedStorage = 20,
            StorageType = Amazon.CDK.AWS.RDS.StorageType.GP3,
            StorageEncrypted = true,
            MultiAz = false,
            PubliclyAccessible = false,
            BackupRetention = Duration.Days(0),
            DeleteAutomatedBackups = true,
            DeletionProtection = false,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        var apiLogs = new LogGroup(this, "ApiLogs", new LogGroupProps
        {
            Retention = RetentionDays.ONE_WEEK,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        var api = new ApplicationLoadBalancedFargateService(this, "Api", new ApplicationLoadBalancedFargateServiceProps
        {
            Vpc = vpc,
            Cpu = 256,
            MemoryLimitMiB = 512,
            DesiredCount = 1,
            MinHealthyPercent = 100,
            MaxHealthyPercent = 200,
            PublicLoadBalancer = true,
            OpenListener = false,
            AssignPublicIp = true,
            TaskSubnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC },
            HealthCheckGracePeriod = Duration.Seconds(60),
            CircuitBreaker = new DeploymentCircuitBreaker { Rollback = true },
            TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
            {
                Image = ContainerImage.FromAsset(Path.Combine(root, "api"), new AssetImageProps
                {
                    Platform = ImagePlatform.LINUX_AMD64
                }),
                ContainerPort = 8080,
                Environment = new Dictionary<string, string>
                {
                    ["ConnectionStrings__BookStore"] =
                        $"Host={database.DbInstanceEndpointAddress};Port={database.DbInstanceEndpointPort};Database={DatabaseName};Username={DatabaseUser};GSS Encryption Mode=Disable"
                },
                Secrets = new Dictionary<string, EcsSecret>
                {
                    ["PGPASSWORD"] = EcsSecret.FromSecretsManager(database.Secret!, "password")
                },
                LogDriver = LogDrivers.AwsLogs(new AwsLogDriverProps
                {
                    StreamPrefix = "api",
                    LogGroup = apiLogs
                })
            }
        });

        api.Node.TryRemoveChild("LoadBalancerDNS");
        api.Node.TryRemoveChild("ServiceURL");

        api.TargetGroup.ConfigureHealthCheck(new ElbHealthCheck
        {
            Path = "/health",
            HealthyHttpCodes = "200"
        });

        api.LoadBalancer.Connections.AllowFrom(Peer.PrefixList(CloudFrontOriginFacingPrefixList), Port.Tcp(80));
        database.Connections.AllowDefaultPortFrom(api.Service);

        var bucket = new Bucket(this, "WebBucket", new BucketProps
        {
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            Encryption = BucketEncryption.S3_MANAGED,
            EnforceSSL = true,
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true
        });

        var spaRouting = new Function(this, "SpaRouting", new FunctionProps
        {
            Runtime = FunctionRuntime.JS_2_0,
            Code = FunctionCode.FromInline("""
                function handler(event) {
                  var request = event.request;
                  if (!request.uri.split('/').pop().includes('.')) {
                    request.uri = '/index.html';
                  }
                  return request;
                }
                """)
        });

        var distribution = new Distribution(this, "Distribution", new DistributionProps
        {
            DefaultRootObject = "index.html",
            PriceClass = PriceClass.PRICE_CLASS_100,
            DefaultBehavior = new BehaviorOptions
            {
                Origin = S3BucketOrigin.WithOriginAccessControl(bucket),
                ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                CachePolicy = CachePolicy.CACHING_OPTIMIZED,
                FunctionAssociations =
                [
                    new FunctionAssociation { Function = spaRouting, EventType = FunctionEventType.VIEWER_REQUEST }
                ]
            },
            AdditionalBehaviors = new Dictionary<string, IBehaviorOptions>
            {
                ["/api/*"] = new BehaviorOptions
                {
                    Origin = new LoadBalancerV2Origin(api.LoadBalancer, new LoadBalancerV2OriginProps
                    {
                        ProtocolPolicy = OriginProtocolPolicy.HTTP_ONLY
                    }),
                    ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                    AllowedMethods = AllowedMethods.ALLOW_ALL,
                    CachePolicy = CachePolicy.CACHING_DISABLED,
                    OriginRequestPolicy = OriginRequestPolicy.ALL_VIEWER_EXCEPT_HOST_HEADER
                }
            }
        });

        new BucketDeployment(this, "WebDeployment", new BucketDeploymentProps
        {
            Sources = [Source.Asset(Path.Combine(root, "web", "dist", "web", "browser"))],
            DestinationBucket = bucket,
            Distribution = distribution,
            DistributionPaths = ["/*"],
            LogGroup = new LogGroup(this, "WebDeploymentLogs", new LogGroupProps
            {
                Retention = RetentionDays.ONE_WEEK,
                RemovalPolicy = RemovalPolicy.DESTROY
            })
        });

        var reviewerPassword = new SecretsManagerSecret(this, "ReviewerPassword", new SecretProps
        {
            Description = "Console password for btx-reviewer",
            GenerateSecretString = new SecretStringGenerator
            {
                PasswordLength = 24,
                RequireEachIncludedType = true,
                ExcludeCharacters = "\"'`\\/@ "
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        var reviewer = new User(this, "Reviewer", new UserProps
        {
            UserName = "btx-reviewer",
            Password = reviewerPassword.SecretValue,
            PasswordResetRequired = true,
            ManagedPolicies =
            [
                ManagedPolicy.FromAwsManagedPolicyName("job-function/ViewOnlyAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("CloudWatchReadOnlyAccess"),
                ManagedPolicy.FromAwsManagedPolicyName("IAMUserChangePassword")
            ]
        });

        new Policy(this, "ReviewerGuardrails", new PolicyProps
        {
            Users = [reviewer],
            Statements =
            [
                new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.DENY,
                    Actions = ["ssm:GetParameter*", "secretsmanager:GetSecretValue"],
                    Resources = ["*"]
                }),
                new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.DENY,
                    Actions =
                    [
                        "aws-portal:*",
                        "billing:*",
                        "budgets:*",
                        "ce:*",
                        "cur:*",
                        "freetier:*",
                        "invoicing:*",
                        "payments:*",
                        "purchase-orders:*",
                        "tax:*"
                    ],
                    Resources = ["*"]
                }),
                new PolicyStatement(new PolicyStatementProps
                {
                    Effect = Effect.DENY,
                    Actions = ["*"],
                    Resources = ["*"],
                    Conditions = new Dictionary<string, object>
                    {
                        ["DateGreaterThan"] = new Dictionary<string, object> { ["aws:CurrentTime"] = "2026-11-25T00:00:00Z" }
                    }
                })
            ]
        });

        new CfnOutput(this, "CloudFrontUrl", new CfnOutputProps { Value = $"https://{distribution.DistributionDomainName}" });
        new CfnOutput(this, "ConsoleSignInUrl", new CfnOutputProps { Value = $"https://{Aws.ACCOUNT_ID}.signin.aws.amazon.com/console" });
        new CfnOutput(this, "ReviewerPasswordSecretName", new CfnOutputProps { Value = reviewerPassword.SecretName });
    }
}
