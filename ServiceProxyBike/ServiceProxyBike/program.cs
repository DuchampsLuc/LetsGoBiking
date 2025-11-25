using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace ServiceProxyBike
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8123/Service1/");
        using (ServiceHost host = new ServiceHost(typeof(Service1), baseAddress))
            {
                try
                {
                    // Créer un BasicHttpBinding avec des quotas augmentés pour les gros messages  
                    BasicHttpBinding binding = new BasicHttpBinding();
                    binding.MaxReceivedMessageSize = 1024 * 1024 * 10; // 10 Mo  
                    binding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 10; // 10 Mo  

                    // Ajouter le endpoint du service  
                    host.AddServiceEndpoint(typeof(IService1), binding, "");

                    // Ajouter le endpoint de metadata (MEX)  
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

                    Console.WriteLine("→ Ouverture du service…");
                    host.Open();
                    Console.WriteLine("Serveur SOAP démarré !");
                    Console.WriteLine("Adresse : http://localhost:8123/Service1/");
                    Console.WriteLine("WSDL   : http://localhost:8123/Service1/mex");
                    Console.WriteLine("Appuyez sur ENTER pour arrêter.");
                    Console.ReadLine();

                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Erreur au démarrage du serveur :");
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }  


}
