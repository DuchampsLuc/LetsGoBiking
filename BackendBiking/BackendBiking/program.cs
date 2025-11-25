using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace BackendBiking
{
    class Program
    {
        static void Main(string[] args)
        {
            // Adresse de base pour le service
            Uri baseAddress = new Uri("http://localhost:8200/Service1/");
        // Créer le self-host
        using (ServiceHost host = new ServiceHost(typeof(Service1), baseAddress))
            {
                try
                {
                    // Binding SOAP avec quotas élevés
                    BasicHttpBinding soapBinding = new BasicHttpBinding
                    {
                        MaxReceivedMessageSize = 2147483647,
                        ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max
                    };

                    // Endpoint SOAP
                    host.AddServiceEndpoint(typeof(IService1), soapBinding, "");

                    // Endpoint REST
                    WebHttpBinding restBinding = new WebHttpBinding
                    {
                        MaxReceivedMessageSize = 2147483647
                    };
                    ServiceEndpoint restEndpoint = host.AddServiceEndpoint(typeof(IService1), restBinding, "rest");
                    restEndpoint.Behaviors.Add(new WebHttpBehavior());

                    // Endpoint MEX pour Visual Studio
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior
                    {
                        HttpGetEnabled = true
                    };
                    host.Description.Behaviors.Add(smb);
                    host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

                    // Ouverture du service
                    host.Open();
                    Console.WriteLine("→ Serveur BackendBiking démarré !");
                    Console.WriteLine("Adresse SOAP : http://localhost:8200/Service1/");
                    Console.WriteLine("Adresse REST : http://localhost:8200/Service1/rest");
                    Console.WriteLine("WSDL / MEX : http://localhost:8200/Service1/mex");
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
