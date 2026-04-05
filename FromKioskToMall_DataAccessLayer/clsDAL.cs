using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FromKioskToMall.Models;

namespace FromKioskToMall_DataAccessLayer
{
    public class KioskDataProvider
    {
        private readonly string _connectionString = "Server=.;Database=FromKioskToMallDB;Trusted_Connection=True;";

        // 1. جلب كل الأصناف (المشروبات، السناكات، إلخ)
        public List<KioskItem> GetCategories()
        {
            var categories = new List<KioskItem>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT ID, Name, Metadata FROM Items WHERE [Type] = 'category' ORDER BY Name";
                var cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        categories.Add(new KioskItem
                        {
                            ID = (Guid)dr["ID"],
                            Name = dr["Name"].ToString(),
                            Metadata = dr["Metadata"].ToString()
                        });
                    }
                }
            }
            return categories;
        }

        // 2. جلب المنتجات التابعة لصنف معين (عبر العلاقة 'contains')
        public List<KioskItem> GetProductsByCategory(Guid categoryId)
        {
            var products = new List<KioskItem>();
            using (var conn = new SqlConnection(_connectionString))
            {
                // هنا يظهر جمال الـ Relations Table
                string sql = @"SELECT
                I.ID, I.Name, I.Metadata, 
                Parent.Name AS ParentName
                FROM Items I
                JOIN Relations R ON I.ID = R.Target_ID
                JOIN Items Parent ON R.Source_ID = Parent.ID
                           WHERE R.Source_ID = @catId AND R.Rel_Type = 'contains'";


                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@catId", categoryId);
                conn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        products.Add(new KioskItem
                        {
                            ID = (Guid)dr["ID"],
                            Name = dr["Name"].ToString(),
                            Metadata = dr["Metadata"].ToString(),
                            ProductCategory = dr["ParentName"].ToString()
                        });
                    }
                }
            }
            return products;
        }
        
        public bool AddProductToCategory(KioskItem newProduct, Guid categoryId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // بدء عملية Transaction لضمان تنفيذ الخطوتين معاً أو فشلهما معاً
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. إدخال المنتج في جدول Items
                        string sqlItem = @"INSERT INTO Items (ID, [Type], [Name], Metadata, CreatedAt) 
                                       VALUES (@id, @type, @name, @metadata, @createdAt)";

                        using (var cmdItem = new SqlCommand(sqlItem, conn, transaction))
                        {
                            cmdItem.Parameters.AddWithValue("@id", newProduct.ID);
                            cmdItem.Parameters.AddWithValue("@type", "product");
                            cmdItem.Parameters.AddWithValue("@name", newProduct.Name);
                            cmdItem.Parameters.AddWithValue("@metadata", newProduct.Metadata);
                            cmdItem.Parameters.AddWithValue("@createdAt", DateTime.Now);
                            cmdItem.ExecuteNonQuery();
                        }

                        // 2. إنشاء العلاقة في جدول Relations (ربط المنتج بالصنف)
                        string sqlRelation = @"INSERT INTO Relations (Source_ID, Target_ID, Rel_Type) 
                                           VALUES (@sourceId, @targetId, @relType)";

                        using (var cmdRel = new SqlCommand(sqlRelation, conn, transaction))
                        {
                            cmdRel.Parameters.AddWithValue("@sourceId", categoryId); // الـ ID الخاص بالصنف
                            cmdRel.Parameters.AddWithValue("@targetId", newProduct.ID); // الـ ID الخاص بالمنتج الجديد
                            cmdRel.Parameters.AddWithValue("@relType", "contains");
                            cmdRel.ExecuteNonQuery();
                        }

                        // اعتماد التغييرات في قاعدة البيانات
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // في حال حدوث أي خطأ، يتم إلغاء كل ما تم تنفيذه داخل الـ Transaction
                        transaction.Rollback();
                        // يمكنك تسجيل الخطأ هنا (Logging)
                        return false;
                    }
                }
            }
        }

        public bool DeleteProduct(Guid productId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. حذف كافة العلاقات المرتبطة بهذا المنتج أولاً
                        // (سواء كان هدفاً Target_ID أو مصدراً Source_ID)
                        string sqlRelations = "DELETE FROM Relations WHERE Source_ID = @id OR Target_ID = @id";
                        using (var cmdRel = new SqlCommand(sqlRelations, conn, transaction))
                        {
                            cmdRel.Parameters.AddWithValue("@id", productId);
                            cmdRel.ExecuteNonQuery();
                        }

                        // 2. حذف المنتج نفسه من جدول Items
                        string sqlItem = "DELETE FROM Items WHERE ID = @id";
                        using (var cmdItem = new SqlCommand(sqlItem, conn, transaction))
                        {
                            cmdItem.Parameters.AddWithValue("@id", productId);
                            cmdItem.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // يمكن إضافة Logging هنا للـ ex.Message
                        return false;
                    }
                }
            }
        }

        public bool UpdateProduct(KioskItem product)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                // نص الاستعلام لتحديث الحقول الأساسية والـ Metadata
                string sql = @"UPDATE Items 
                       SET Name = @name, 
                           Metadata = @metadata,
                           [Type] = @type
                       WHERE ID = @id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", product.ID);
                    cmd.Parameters.AddWithValue("@name", product.Name);
                    cmd.Parameters.AddWithValue("@metadata", product.Metadata);
                    cmd.Parameters.AddWithValue("@type", product.Type);

                    try
                    {
                        conn.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // إذا كان عدد الأسطر المتأثرة أكبر من 0، فالعملية نجحت
                        return rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        // تسجيل الخطأ (Logging) مهم جداً هنا في مرحلة الـ Production
                        return false;
                    }
                }
            }
        }
    }
}