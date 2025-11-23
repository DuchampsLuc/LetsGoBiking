using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Web;
using System.ServiceModel;
using System.Text;
using System.Reflection.Emit;
using System.Threading.Tasks;


namespace BackendBiking
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        [WebInvoke(
            Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "GetData?value={value}")]
        string GetData(int value);
		[WebInvoke(
			Method = "GET",
			ResponseFormat = WebMessageFormat.Json,
			BodyStyle = WebMessageBodyStyle.Wrapped,
			UriTemplate = "GetRoute?start={start}&dest={dest}")]
		Task<string> GetRoute(string start, string dest);

        [WebInvoke(
            Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "GetAdresse?adresse={adresse}")]
		Task<string> GetAdresse(string adresse);

		[WebInvoke(
            Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = "GetCoordonnees?adresse={adresse}")]
		Task<string> GetCoordonnees(string adresse);
    }
}


// Utilisez un contrat de données comme indiqué dans l'exemple ci-après pour ajouter les types composites aux opérations de service.
// Vous pouvez ajouter des fichiers XSD au projet. Une fois le projet généré, vous pouvez utiliser directement les types de données qui y sont définis, avec l'espace de noms "BackendBiking.ContractType".
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
