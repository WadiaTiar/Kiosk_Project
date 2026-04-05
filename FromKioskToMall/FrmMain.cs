using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FromKioskToMall.Models;
using FromKioskToMall_BusinessAccessLayer;

namespace FromKioskToMall
{
    public partial class Main_Form : Form
    {
        private KioskService _kioskService = new KioskService();
        List<KioskItem> currentProductsList;
        private Guid _currentSelectedCategoryId;
        public Main_Form()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadCategories();
        }


        // 1. توليد أزرار الأصناف برمجياً
        private void LoadCategories()
        {
            flpCategories.Controls.Clear();
            var categories = _kioskService.GetMenuCategories();

            foreach (var cat in categories)
            {
                Button btn = new Button();
                btn.Text = cat.Name;
                btn.Width = 120;
                btn.Height = 60;
                btn.Tag = cat.ID; // تخزين الـ GUID الخاص بالصنف داخل الزر

                // ربط حدث الضغط
                btn.Click += CategoryButton_Click;

                flpCategories.Controls.Add(btn);
            }
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            Guid categoryId = (Guid)clickedButton.Tag; // استرجاع الـ ID من الـ Tag

            RefreshProductGrid(categoryId);
        }


        // 3. تحديث الـ DataGridView بالمنتجات
        private void RefreshProductGrid(Guid categoryId)
        {
            _currentSelectedCategoryId = categoryId; // حفظ المعرف هنا
            currentProductsList = _kioskService.GetProductsForCategory(categoryId);

            // سنستخدم List بسيطة لعرض البيانات بشكل جميل بدلاً من الـ Raw Objects
            var displayList = currentProductsList.Select(p => new {
                الاسم = p.Name,
                السعر = p.GetPrice()
            }).ToList();

            dgvProducts.DataSource = displayList;
            
            // تحسين مظهر الجدول
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (dgvProducts.Rows.Count > 0)
            {
                var firstRow = dgvProducts.Rows[0];
                // مثال: الحصول على قيمة خلية معينة في السطر الأول
                string name = firstRow.Cells["الاسم"].Value.ToString();
                UpdateProductDetails(name);
            }            
        }

        private void dgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            // 1. التأكد من وجود سطر مختار
            if (dgvProducts.SelectedRows.Count > 0)
            {
                // 2. الحصول على المنتج المختار من الـ DataSource
                // ملاحظة: إذا كنت تستخدم Anonymous Object في الـ DataSource، 
                // سنحتاج للوصول للمنتج الأصلي. سأفترض أنك قمت بتخزين القائمة الأصلية.

                // لنأخذ اسم المنتج المختار للبحث عنه أو استرجاع بياناته
                string selectedProductName = dgvProducts.SelectedRows[0].Cells["الاسم"].Value.ToString();

                // استدعاء ميثود التحديث
                UpdateProductDetails(selectedProductName);
            }
        }

        private void UpdateProductDetails(string productName)
        {
            lstProductDetails.Items.Clear();

            // ابحث عن المنتج الأصلي في القائمة التي جلبناها من الـ Service
            // (يفضل أن تكون قائمة المنتجات مخزنة في متغير على مستوى الفورم لتجنب ضرب قاعدة البيانات في كل ضغطة)
            var product = currentProductsList.FirstOrDefault(p => p.Name == productName);

            if (product != null)
            {
                // الحصول على القاموس (Dictionary) من الـ Metadata
                var properties = product.GetProperties();

                if (properties.Count > 0)
                {
                    foreach (var prop in properties)
                    {
                        // إضافة السطر بشكل: "اسم الخاصية: القيمة"
                        // مثال: "price: 150" أو "size: 330ml"
                        string detailLine = $"{prop.Key}: {prop.Value}";
                        lstProductDetails.Items.Add(detailLine);
                    }
                }
                else
                {
                    lstProductDetails.Items.Add("لا توجد تفاصيل إضافية.");
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var addForm = new FrmAddProduct())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    // تحديث الواجهة لرؤية المنتج الجديد فوراً
                    LoadCategories();
                }
            }
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            // 1. التأكد من أن المستخدم اختار منتجاً من الجدول
            if (dgvProducts.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار منتج من الجدول أولاً للحذف.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. استخراج بيانات المنتج المختار (الاسم والـ ID)
            // ملاحظة: نفترض أننا قمنا بتخزين الـ ID في عمود مخفي أو الوصول له عبر القائمة الأصلية
            string productName = dgvProducts.SelectedRows[0].Cells["الاسم"].Value.ToString();

            // للحصول على الـ ID، سنبحث عنه في القائمة التي جلبناها سابقاً (currentProductsList)
            var selectedProduct = currentProductsList.FirstOrDefault(p => p.Name == productName);

            if (selectedProduct == null) return;

            // 3. رسالة تأكيد "شديدة اللهجة" لأن الحذف نهائي
            var confirmResult = MessageBox.Show(
                $"هل أنت متأكد من حذف المنتج '{productName}' نهائياً؟\nلا يمكن التراجع عن هذه العملية!",
                "تأكيد الحذف النهائي",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error); // أيقونة الخطأ للتنبيه بالخطورة

            if (confirmResult == DialogResult.Yes)
            {
                // 4. استدعاء الـ Service لتنفيذ الحذف
                bool isDeleted = _kioskService.RemoveProduct(selectedProduct.ID);

                if (isDeleted)
                {
                    MessageBox.Show("تم حذف المنتج بنجاح.");
                    lstProductDetails.Items.Clear();
                    // 5. تحديث الواجهة فوراً (إعادة جلب المنتجات لنفس الصنف الحالي)
                    // سنستخدم الـ Tag الخاص بالزر الذي تم ضغطه سابقاً أو معرف الصنف الحالي
                    RefreshProductGrid(_currentSelectedCategoryId);
                    // نصيحة: يفضل استدعاء ميثود التحديث التي كتبناها سابقاً
                }
                else
                {
                    MessageBox.Show("فشل الحذف، قد يكون المنتج مرتبطاً بسجلات مبيعات أخرى.");
                }
            }
        }

        private void btnEditProduct_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                // 1. تحديد المنتج المختار من القائمة
                string selectedName = dgvProducts.SelectedRows[0].Cells["الاسم"].Value.ToString();
                var productToEdit = currentProductsList.FirstOrDefault(p => p.Name == selectedName);

                if (productToEdit != null)
                {
                    // 2. فتح نافذة التعديل وتمرير الكائن لها
                    using (var editForm = new FrmEditProduct(productToEdit))
                    {
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            // 3. تحديث الجدول فوراً إذا نجح التعديل
                            RefreshProductGrid(_currentSelectedCategoryId);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("يرجى اختيار منتج أولاً لتعديله.");
            }
        }
    }
}
