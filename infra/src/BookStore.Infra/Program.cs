using Amazon.CDK;
using BookStore.Infra;

var app = new App();

new BookStoreStack(app, "BookStoreStack", new StackProps
{
    Env = new Amazon.CDK.Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = "us-east-1"
    }
});

Tags.Of(app).Add("Project", "BookStore");

app.Synth();
