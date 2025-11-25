using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ServiceProxyBike
{
    class Program
    {
        static void Main(string[] args)
        {
            // Adresse du service
            Uri baseAddress = new Uri("http://localhost:8000/Service1/");

            // Créer et configurer le self-host
            using (ServiceHost host = new ServiceHost(typeof(Service1), baseAddress))
            {
                // Ajouter un endpoint basicHttpBinding
                host.AddServiceEndpoint(
                    typeof(IService1),
                    new BasicHttpBinding(),
                    ""
                );

                // Ajouter un endpoint MEX
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true
                };
                host.Description.Behaviors.Add(smb);
                host.AddServiceEndpoint(
                    typeof(IMetadataExchange),
                    MetadataExchangeBindings.CreateMexHttpBinding(),
                    "mex"
                );

                // Ouvrir le service
                host.Open();
                Console.WriteLine("Serveur SOAP démarré !");
                Console.WriteLine("Adresse : http://localhost:8080/Service1/");
                Console.WriteLine("Appuyez sur ENTER pour arrêter.");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}
