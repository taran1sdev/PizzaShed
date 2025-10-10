using Microsoft.Data.SqlClient;
using PizzaShed.Model;
using PizzaShed.Services.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;

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
                                        Postcode = postcode,
                                        Flat = flatNo,
                                        House = houseNo.ToString(),
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
        
        
        public bool UpdateCustomer(Customer customer)
        {
            string queryString = @"
                UPDATE Customers
                SET name = @name,
                    phone_no = @number,
                    post_code = @postcode,
                    flat_no = @flat,
                    house_no = @houseNo,
                    street_address = @streetAddress,
                    delivery_notes = @deliveryNotes
                WHERE customer_id = @customerID;";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@name", customer.Name);
                        query.Parameters.AddWithValue("@number", customer.PhoneNumber);
                        query.Parameters.AddWithValue("@postcode", customer.Postcode);
                        query.Parameters.AddWithValue("@houseNo", customer.HouseNo);
                        query.Parameters.AddWithValue("@streetAddress", customer.StreetAddress);
                        query.Parameters.AddWithValue("@customerID", customer.ID);


                        // We handle the possible null parameters here
                        if (customer.Flat != null)
                        {
                            query.Parameters.AddWithValue("@flat", customer.Flat);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@flat", DBNull.Value);
                        }

                        if (customer.DeliveryNotes != null)
                        {
                            query.Parameters.AddWithValue("@deliveryNotes", customer.DeliveryNotes);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@deliveryNotes", DBNull.Value);
                        }

                        if (query.ExecuteNonQuery() > 0)
                        {
                            EventLogger.LogInfo("Customer record updated");
                            return true;
                        }
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to update customer record: " + ex.Message);
            }
            return false;
        }
        
        // This function creates a new customer record and returns the customer_id field
        public int CreateNewCustomer(Customer customer)
        {
            string queryString = @"
                INSERT INTO Customers (name, phone_no, post_code, flat_no, house_no, street_address, delivery_notes)
                OUTPUT Inserted.customer_id
                VALUES (
                        @name, 
                        @number, 
                        @postcode, 
                        @flat, 
                        @houseNo,   
                        @streetAddress,
                        @deliveryNotes);";

            try
            {
                return _databaseManager.ExecuteQuery(conn =>
                {
                    using (SqlCommand query = new SqlCommand(queryString, conn))
                    {
                        query.Parameters.AddWithValue("@name", customer.Name);
                        query.Parameters.AddWithValue("@number", customer.PhoneNumber);
                        query.Parameters.AddWithValue("@postcode", customer.Postcode);
                        query.Parameters.AddWithValue("@houseNo", customer.HouseNo);
                        query.Parameters.AddWithValue("@streetAddress", customer.StreetAddress);


                        // We handle the possible null parameters here
                        if (customer.Flat != null)
                        {
                            query.Parameters.AddWithValue("@flat", customer.Flat);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@flat", DBNull.Value);
                        }

                        if (customer.DeliveryNotes != null)
                        {
                            query.Parameters.AddWithValue("@deliveryNotes", customer.DeliveryNotes);
                        }
                        else
                        {
                            query.Parameters.AddWithValue("@deliveryNotes", DBNull.Value);
                        }

                        using (SqlDataReader reader = query.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                EventLogger.LogInfo("Successfully created new customer: " + customer.Name);
                                return reader.IsDBNull(reader.GetOrdinal("customer_id")) ? 0 : Convert.ToInt32(reader["customer_id"]);
                            }
                            return -1;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                EventLogger.LogError("Failed to create new customer " + ex.Message);
            }
            return 0;
        }
    }
}
