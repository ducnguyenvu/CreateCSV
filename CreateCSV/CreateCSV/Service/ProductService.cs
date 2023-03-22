/*using CreateCSV.Model;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Collections;

namespace CreateCSV.Service
{
    public class ProductService
    {
        public List<Product> GetAllProductsAsync()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Connection21"].ConnectionString;
            string query = "SELECT m.ShopifyId[Shopify Collection ID], c.CategoryName[Shopify Collection Name] " +
                "from beta_swimoutlet_com_standalone.dbo.SOT_SHOPIFY_IDS_MAPPING m with(nolock)" +
                "join Categories c with(nolock) on m.OurId = c.CategoryID\r\nwhere m.TableName='categories' and m.OurSite='Yogaoutlet' ORDER by c.CategoryID"   
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
    }
}
*/