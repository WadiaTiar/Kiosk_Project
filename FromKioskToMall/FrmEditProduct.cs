using FromKioskToMall.Models;
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

namespace FromKioskToMall
{
    public partial class FrmEditProduct : Form
    {
        private KioskService _service = new KioskService();
        private Guid _productId; // لتخزين الـ ID الخاص بالمنتج الذي نعدله

        public FrmEditProduct(KioskItem product)
        {
            InitializeComponent();
            _productId = product.ID;

            // 1. ملء الخانات بالبيانات الحالية
            txtName.Text = product.Name;
            numPrice.Value = product.GetPrice();

            cmbCategories.Text = product.ProductCategory;
            // 2. ملء الـ Grid بالخصائص من الـ Metadata
            SetupAndFillGrid(product);
        }

        private void SetupAndFillGrid(KioskItem product)
        {
            dgvExtraDetails.Columns.Add("Key", "الخاصية");
            dgvExtraDetails.Columns.Add("Value", "القيمة");

            var props = product.GetProperties(); // الميثود التي كتبناها سابقاً لتحليل JSON
            foreach (var p in props)
            {
                // نتخطى السعر لأنه موجود في NumericUpDown منفصل
                if (p.Key.ToLower() == "price") continue;
                dgvExtraDetails.Rows.Add(p.Key, p.Value);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. تجميع الخصائص الجديدة من الـ Grid
            var updatedDetails = new Dictionary<string, object>();
            foreach (DataGridViewRow row in dgvExtraDetails.Rows)
            {
                if (row.Cells["Key"].Value != null)
                {
                    updatedDetails[row.Cells["Key"].Value.ToString()] = row.Cells["Value"].Value;
                }
            }

            // 2. استدعاء الـ BAL (KioskService)
            bool success = _service.UpdateProductDetails(_productId, txtName.Text, numPrice.Value, updatedDetails);

            if (success)
            {
                MessageBox.Show("تم تحديث المنتج بنجاح!");
                this.DialogResult = DialogResult.OK; // لإخبار الشاشة الرئيسية بالنجاح
                this.Close();
            }
            else
            {
                MessageBox.Show("فشل التحديث، يرجى المحاولة لاحقاً.");
            }
        }
    }
}
