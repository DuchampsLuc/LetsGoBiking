using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceProxyBike
{
	// REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom d'interface "IService1" à la fois dans le code et le fichier de configuration.
	[ServiceContract]
	public interface IService1
	{
		[OperationContract]
		Task<string> GetContract(string adresse);

        [OperationContract]
        Task<string> GetCoordonees(string adresse);

		[OperationContract]

		Task<string> getParcours(double lat1, double lng1, double lat2, double lng2, bool isCycling);

		[OperationContract]
		Task<String> getAdresse(string adresse);

        [OperationContract]
        Task<String> getMeteo(double lat,double lng);



        // TODO: ajoutez vos opérations de service ici
    }

	// Utilisez un contrat de données comme indiqué dans l'exemple ci-après pour ajouter les types composites aux opérations de service.
	// Vous pouvez ajouter des fichiers XSD au projet. Une fois le projet généré, vous pouvez utiliser directement les types de données qui y sont définis, avec l'espace de noms "ServiceProxyBike.ContractType".
	[DataContract]
	public class CompositeType
	{
		bool boolValue = true;
		string stringValue = "Hello ";

		[DataMember]
		public bool BoolValue
		{
			get { return boolValue;}
			set { boolValue = value;}
		}

		[DataMember]
		public string StringValue
		{
			get { return stringValue;}
			set { stringValue = value;}
		}
	}
}
