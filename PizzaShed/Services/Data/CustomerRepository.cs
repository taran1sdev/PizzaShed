using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System.Collections.ObjectModel;

namespace PizzaShed.Services.Data
{
    public class CustomerRepository : ICustomerRepository
    {
        IDatabaseManager _databaseManager;
        
        public CustomerRepository(IDatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        // This function searches the database for a customer with a partial name
        // The row returned (if any) will populate a listview for quick selection
        public ObservableCollection<Customer> GetCustomerByPartialName(string partialName)
        {
            string queryString = @"
                SELECT TOP(1) * FROM Customers
                WHERE name LIKE @partialName + '%';
            ";

            try
            {
                Customer? customer = _databaseManager.ExecuteQuery(conn =>
                {

                    using (SqlCommand query = new(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@partialName", partialName);

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                int customerID = reader.IsDBNull(reader.GetOrdinal("customer_id")) ? 0 : Convert.ToInt32(reader["customer_id"]);
                                string? name = reader.IsDBNull(reader.GetOrdinal("name")) ? null : reader["name"].ToString();
                                string? phoneNo = reader.IsDBNull(reader.GetOrdinal("phone_no")) ? null : reader["phone_no"].ToString();
                                string? postcode = reader.IsDBNull(reader.GetOrdinal("post_code")) ? null : reader["post_code"].ToString();
                                string? flatNo = reader.IsDBNull(reader.GetOrdinal("flat_no")) ? null : reader["flat_no"].ToString();
                                int houseNo = reader.IsDBNull(reader.GetOrdinal("house_no")) ? 0 : Convert.ToInt32(reader["house_no"]);
                                string? address = reader.IsDBNull(reader.GetOrdinal("street_address")) ? null : reader["street_address"].ToString();
                                string? deliveryNotes = reader.IsDBNull(reader.GetOrdinal("delivery_notes")) ? null : reader["delivery_notes"].ToString();

                                if (
                                customerID != 0
                                && name != null
                                && phoneNo != null
                                && postcode != null
                                && houseNo != 0
                                && address != null)
                                {
                                    return new Customer
                                    {
                                        ID = customerID,
                                        Name = name,
                                        PhoneNumber = phoneNo,
                                        PostCode = postcode,
                                        FlatNo = flatNo,
                                        HouseNo = houseNo,
                                        StreetAddress = address,
                                        DeliveryNotes = deliveryNotes
                                    };
                                }
                            }
                            return null;
                        }
                    }
                });
                if (customer == null)
                    return [];

                return [customer];
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Error occured fetching customer by name: " + ex.Message);
            }
            return [];
        }
        
        
        
        

        public bool CreateNewCustomer(Customer customer)
        {
            return false;
        }
    }
}
