namespace PRO_Checkers.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSignalR(options =>
            {
                options.MaximumReceiveMessageSize = 1024000;
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
            });


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();

            app.MapHub<GameHub>("game-hub");

            //app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
