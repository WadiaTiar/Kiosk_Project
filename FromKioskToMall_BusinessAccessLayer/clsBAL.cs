using FromKioskToMall.Models;
using FromKioskToMall_DataAccessLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FromKioskToMall_BusinessAccessLayer
{

    public class KioskItem_BAL
    {
        public Guid ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Metadata { get; set; }
    }

    public class KioskService
    {
        private readonly KioskDataProvider _dataProvider = new KioskDataProvider();

        // 1. جلب قائمة الأصناف للواجهة الجانبية
        public List<KioskItem> GetMenuCategories()
        {
            // هنا يمكننا إضافة منطق ترتيب معين أو تصفية الأصناف النشطة فقط
            return _dataProvider.GetCategories();
        }

        // 2. جلب المنتجات لعرضها في الشبكة المركزية
        public List<KioskItem> GetProductsForCategory(Guid categoryId)
        {
            return _dataProvider.GetProductsByCategory(categoryId);
        }

        public bool CreateNewProduct(string name, decimal price, Dictionary<string, object> extraDetails, Guid categoryId)
        {
            // 1. تجميع الخصائص الأساسية في القاموس
            // نضمن دائماً وجود السعر في الـ Metadata
            var metadataDict = extraDetails ?? new Dictionary<string, object>();

            if (!metadataDict.ContainsKey("price"))
                metadataDict.Add("price", price);
            else
                metadataDict["price"] = price;

            // 2. تحويل القاموس إلى نص JSON (هنا تحدث السحر!)
            string jsonMetadata = JsonConvert.SerializeObject(metadataDict);

            // 3. إنشاء كائن الكشك الجديد بـ GUID فريد
            var newProduct = new KioskItem
            {
                ID = Guid.NewGuid(),
                Type = "product",
                Name = name,
                Metadata = jsonMetadata
            };

            // 4. إرسال الكائن للـ DAL ليقوم بعملية الـ Transaction
            return _dataProvider.AddProductToCategory(newProduct, categoryId);
        }

        public bool RemoveProduct(Guid productId)
        {
            // 1. منطق العمل (Business Logic): 
            // يمكننا هنا التحقق من صلاحيات المستخدم أو التأكد أن المنتج ليس "أساسياً"
            if (productId == Guid.Empty)
            {
                return false;
            }

            // 2. استدعاء الـ DAL لتنفيذ الحذف المادي من قاعدة البيانات
            return _dataProvider.DeleteProduct(productId);
        }

        public bool UpdateProductDetails(Guid id, string newName, decimal newPrice, Dictionary<string, object> updatedDetails)
        {
            // 1. التحقق من وجود المعرف
            if (id == Guid.Empty) return false;

            // 2. تجهيز الـ Metadata: دمج السعر مع الخصائص الأخرى
            var metadataDict = updatedDetails ?? new Dictionary<string, object>();

            // نضمن أن السعر الجديد تم تحديثه داخل القاموس
            if (metadataDict.ContainsKey("price"))
                metadataDict["price"] = newPrice;
            else
                metadataDict.Add("price", newPrice);

            // 3. تحويل القاموس المحدث إلى نص JSON
            string jsonMetadata = JsonConvert.SerializeObject(metadataDict);

            // 4. إنشاء كائن المنتج المحدث
            var updatedProduct = new KioskItem
            {
                ID = id,
                Name = newName,
                Metadata = jsonMetadata,
                Type = "product" // نحافظ على النوع ثابتاً
            };

            // 5. إرسال الكائن للـ DAL للتنفيذ في قاعدة البيانات
            return _dataProvider.UpdateProduct(updatedProduct);
        }
    }
}