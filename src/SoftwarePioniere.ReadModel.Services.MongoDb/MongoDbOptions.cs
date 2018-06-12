namespace SoftwarePioniere.ReadModel.Services.MongoDb
{
    public class MongoDbOptions
    {

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DatabaseId { get; set; } = "sopidev";

        public string Server { get; set; } = "localhost";

        public int Port { get; set; } = 27017;

        public override string ToString()
        {
            return $"Server: {Server} // Port: {Port} // DatabaseId: {DatabaseId} // UserName: {UserName} ";
        }

    }
}
