using FromKioskToMall_BusinessAccessLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FromKioskToMall
{
    public partial class FrmAddProduct : Form
    {
        private KioskService _service = new KioskService();
        public FrmAddProduct()
        {
            InitializeComponent();
            LoadCategories();
            SetupDetailsGrid();
        }

        private void SetupDetailsGrid()
        {
            // إعداد جدول الخصائص الإضافية عمودين فقط
            dgvExtraDetails.Columns.Add("Key", "الخاصية (مثل: الوزن)");
            dgvExtraDetails.Columns.Add("Value", "القيمة (مثل: 50g)");
            dgvExtraDetails.AllowUserToAddRows = true; // السماح للمستخدم بإضافة أسطر جديدة
        }

        private void LoadCategories()
        {
            cmbCategories.DataSource = _service.GetMenuCategories();
            cmbCategories.DisplayMember = "Name";
            cmbCategories.ValueMember = "ID";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. جمع الخصائص الإضافية من الـ DataGridView
            var extraDetails = new Dictionary<string, object>();

            foreach (DataGridViewRow row in dgvExtraDetails.Rows)
            {
                if (row.Cells["Key"].Value != null && row.Cells["Value"].Value != null)
                {
                    string key = row.Cells["Key"].Value.ToString();
                    string val = row.Cells["Value"].Value.ToString();
                    extraDetails[key] = val;
                }
            }

            // 2. استدعاء الـ Business Layer
            string name = txtName.Text;
            decimal price = numPrice.Value;
            Guid categoryId = (Guid)cmbCategories.SelectedValue;

            bool success = _service.CreateNewProduct(name, price, extraDetails, categoryId);

            if (success)
            {
                MessageBox.Show("تمت إضافة المنتج بنجاح!");
                this.DialogResult = DialogResult.OK; // لإخبار الـ Form الرئيسية بضرورة التحديث
                this.Close();
            }
            else
            {
                MessageBox.Show("حدث خطأ أثناء الحفظ.");
            }
        }


    }
}