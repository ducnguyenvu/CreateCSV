using CreateCSV.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CreateCSV
{
    internal class Program
    {
        private const string PathToServiceAccountKeyFile = "testcreatecsv-c96d103fe5c1.json";
        private const string ServiceAccountEmail = "testcreatecsv1@testcreatecsv.iam.gserviceaccount.com";
        private const string UploadFileProductEY = "ListProductEY.csv";
        private const string UploadFileProductSO = "ListProductSO.csv";
        private const string path = @"E:\Spiraledge\CreateCSV_New\CreateCSV\CreateCSV\CreateCSV\bin\Debug";
        private const string DirectoryId = "1BXAwnZQJz3V3vartHdY9562aNPxUuVUD";
        public static async Task Export(List<Product> products, string site)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Collection_ID,Collection_Name");
            foreach (var prod in products)
            {
                stringBuilder.AppendLine($"{prod.CollectionID},{prod.CollectionName}");
            }
            // export file csv to folder
            if (site == "EY")
            {
                File.WriteAllText("ListProductEY.csv", stringBuilder.ToString());
            }
            else
            {
                File.WriteAllText("ListProductSO.csv", stringBuilder.ToString());
            }
        }
        public static List<Product> GetAllProductsSO()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Connection21"].ConnectionString;
            //var query = "SELECT m.ShopifyId[CollectionID], c.CategoryName[CollectionName] " +
            //    "from beta_swimoutlet_com_standalone.dbo.SOT_SHOPIFY_IDS_MAPPING_SWIMOUTLET_COM m with(nolock)" +
            //    "join Categories c with(nolock) on m.OurId = c.CategoryID where m.TableName='categories' and m.OurSite='Swimoutlet' ORDER by c.CategoryID";
            var query = "select ProductId[CollectionID], NameProduct[CollectionName] from [dbo].[Product]";
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Product product = new Product
                    {
                        CollectionID = reader["CollectionID"].ToString(),
                        CollectionName = reader["CollectionName"].ToString()
                    };
                    products.Add(product);
                }
                reader.Close();
            }
            return products;
        }
        public static List<Product> GetAllProductsEY()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Connection21"].ConnectionString;
            var query = "SELECT m.ShopifyId[CollectionID], c.CategoryName[CollectionName] " +
                "from beta_swimoutlet_com_standalone.dbo.SOT_SHOPIFY_IDS_MAPPING m with(nolock)" +
                "join Categories c with(nolock) on m.OurId = c.CategoryID where m.TableName='categories' and m.OurSite='Yogaoutlet' ORDER by c.CategoryID";
            List<Product> products = new List<Product>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Product product = new Product
                    {
                        CollectionID = reader["CollectionID"].ToString(),
                        CollectionName = reader["CollectionName"].ToString()
                    };
                    products.Add(product);
                }
                reader.Close();
            }
            return products;
        }
        public static async Task Upload(string uploadFileProduct)
        {
            // Load the Service account credentials and define the scope of its access.
            var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFile)
                            .CreateScoped(DriveService.ScopeConstants.Drive);
            // Create the  Drive service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            // Upload file Metadata
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = uploadFileProduct,
                Parents = new List<string>() { DirectoryId }
            };

            FilesResource.ListRequest listRequest = service.Files.List();
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
            var uploadedFile = files.FirstOrDefault(f => f.Name == uploadFileProduct);
            if (uploadedFile == null)
            {
                // find the file we uploaded by name 
                using (var fsSource = new FileStream(uploadFileProduct, FileMode.Open, FileAccess.Read))
                {
                    // Create a new file, with metadata and stream.
                    var request = service.Files.Create(fileMetadata, fsSource, "text/csv");
                    request.Fields = "*";
                    var results = await request.UploadAsync(CancellationToken.None);
                }
            }
            else
            {
                //// Note: not all fields are writeable watch out, you cant just send uploadedFile back.
                var updateFileBody = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = uploadFileProduct,
                };
                //// Then upload the file again with a new name and new data.
                using (var uploadStream = new FileStream(uploadFileProduct, FileMode.Open, FileAccess.Read))
                {
                    // Update the file id, with new metadata and stream.
                    var updateRequest = service.Files.Update(updateFileBody, uploadedFile.Id, uploadStream, "text/csv");
                    var result = await updateRequest.UploadAsync(CancellationToken.None);
                }
            }
        }
        static async Task Main(string[] args)
        {
            await Export(GetAllProductsSO(), "SO");
            //await Export(GetAllProductsEY(),"EY");
            //await Upload(UploadFileProductEY);
            await Upload(UploadFileProductSO);
        }

    }
}
