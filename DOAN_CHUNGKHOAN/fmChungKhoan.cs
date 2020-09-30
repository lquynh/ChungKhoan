using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DOAN_CHUNGKHOAN
{
    public partial class fmChungKhoan : Form
    {
        private char loaiGD;

        private int changeCount = 0;
        private const string tableName = "Tructuyen";
        //private const string statusMessage = "Đã có {0} thay đổi.";

        private SqlConnection connection = null;
        private SqlCommand command = null;
        private DataSet dataToWatch = null;

        private bool CanRequestNotifications()
        {
            try
            {
                SqlClientPermission perm = new SqlClientPermission(PermissionState.Unrestricted);

                perm.Demand();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Notification", MessageBoxButtons.OK);
                return false;
            }
        }

        private string GetConnectionString()
        {
            //return "Data Source=MSI;Initial Catalog=CHUNGKHOAN;User ID=sa;Password=0909";
            return "Data Source=LQUYNH;Initial Catalog=CHUNGKHOAN;User ID=sa;Password=1";
        }

        private string GetSQL()
        {
            return "SELECT MACP AS [MACP], GIA2_DM AS [Giá 2 Mua], KL2_DM AS [Khối Lượng 2 Mua], GIA1_DM AS [Giá 1 Mua], KL1_DM AS [Khối Lượng 1 Mua], GIAKHOP AS [Giá Khớp], KL_KHOP AS [Khối Lượng Khớp], GIA1_DB AS [Giá 1 DB], KL1_DB AS [Khối Lượng 1 Bán], GIA2_DB AS [Giá 2 Bán], KL2_DB AS [Khối Lượng 2 Bán] FROM dbo.TRUCTUYEN";
        }

        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            ISynchronizeInvoke i = (ISynchronizeInvoke)this;
            if (i.InvokeRequired)
            {
                OnChangeEventHandler tempDelegate = new OnChangeEventHandler(dependency_OnChange);
                object[] args = new[] { sender, e };
                i.BeginInvoke(tempDelegate, args);
                return;
            }
            SqlDependency dependency = (SqlDependency)sender;
            dependency.OnChange -= dependency_OnChange;
            changeCount += 1;
            //this.label5.Text = string.Format(statusMessage, changeCount);
            {
                var withBlock = this.listBox2.Items;
                withBlock.Clear();
                withBlock.Add("Info:   " + e.Info.ToString());
                withBlock.Add("Source: " + e.Source.ToString());
                withBlock.Add("Type:   " + e.Type.ToString());
            }
            GetData();
        }

        private void GetData()
        {
            dataToWatch.Clear();
            command.Notification = null;
            SqlDependency dependency = new SqlDependency(command);
            dependency.OnChange += dependency_OnChange;

            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(dataToWatch, tableName);
                this.dataGridView1.DataSource = dataToWatch;
                this.dataGridView1.DataMember = tableName;
            }
        }

        private void BatDau()
        {
            changeCount = 0;
            SqlDependency.Stop(GetConnectionString());
            try
            {
                SqlDependency.Start(GetConnectionString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Notification", MessageBoxButtons.OK);
                return;
            }

            if (connection == null)
            {
                connection = new SqlConnection(GetConnectionString());
                connection.Open();
            }
            if (command == null)
                command = new SqlCommand(GetSQL(), connection);

            if (dataToWatch == null)
                dataToWatch = new DataSet();
            GetData();
        }

        public fmChungKhoan()
        {
            InitializeComponent();
            if (CanRequestNotifications() == true)
                BatDau();
            else
                MessageBox.Show("Bạn chưa kích hoạt dịch vụ Broker", "Notification", MessageBoxButtons.OK);

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SqlDependency.Stop(GetConnectionString());
            if (connection != null)
                connection.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'cHUNGKHOANDataSet.LENHDAT' table. You can move, or remove it, as needed.
            //this.lENHDATTableAdapter.Fill(this.cHUNGKHOANDataSet.LENHDAT);
            // TODO: This line of code loads data into the 'cHUNGKHOANDataSet1.LENHDAT' table. You can move, or remove it, as needed.
            this.lENHDATTableAdapter1.Fill(this.cHUNGKHOANDataSet1.LENHDAT);
        }


        private void btnDatLenh_Click(object sender, EventArgs e)
        {
            if (radioMua.Checked == false && radioBan.Checked == false)
            {
                MessageBox.Show("Vui lòng chọn hình thức Mua hoặc Bán", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (txtMaCP.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập mã Cổ Phiếu!", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (txtSoLuong.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập số lượng mua Cổ Phiếu!", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (txtGiaDat.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập giá đặt !", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (Int32.Parse(txtSoLuong.Text.Trim()) == 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (Int32.Parse(txtGiaDat.Text.Trim()) == 0)
            {
                MessageBox.Show("Giá đặt phải lớn hơn 0!", "Thông báo", MessageBoxButtons.OK);
                return;
            }
            if (radioMua.Checked == true) loaiGD = 'M';
            else loaiGD = 'B';

            if (Program.KetNoi() == 0) return;
            string strLenh = "EXEC SP_KHOPLENH_LO '" + txtMaCP.Text + "','" + dtpNgay.Value.ToString("dd-MM-yyyy")+ "','" + loaiGD + "'," + txtSoLuong.Text + "," + txtGiaDat.Text;

            try
            {
                Program.myReader = Program.ExecSqlDataReader(strLenh);
                if (Program.myReader != null)
                {
                    Program.myReader.Read();
                    Int32 ret = Program.myReader.GetInt32(0);
                    //MessageBox.Show(ret.ToString());
                    MessageBox.Show("Đặt lệnh thành công, số cổ phiếu khớp là:  "+ret.ToString(), "Thông báo", MessageBoxButtons.OK);
                    //this.lENHDATTableAdapter.Fill(this.cHUNGKHOANDataSet.LENHDAT);
                    this.lENHDATTableAdapter1.Fill(this.cHUNGKHOANDataSet1.LENHDAT);
                }


            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                MessageBox.Show("Lỗi đặt lệnh!\n" + ex.Message, "Thông báo",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            txtMaCP.Text = "";
            txtGiaDat.Text = "";
            txtSoLuong.Text = "";
            if (Program.myReader == null) return;
            Program.conn.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Program.KetNoi()  == 0) return;
            string strLenh = "EXEC SP_RESET_TTL " +
                             "EXEC SP_CLEARDATE_TRUCTUYEN";

            try
            {
                Program.myReader = Program.ExecSqlDataReader(strLenh);
                MessageBox.Show("Reset thành công ", "Thông báo", MessageBoxButtons.OK);
                //this.lENHDATTableAdapter.Fill(this.cHUNGKHOANDataSet.LENHDAT);
                this.lENHDATTableAdapter1.Fill(this.cHUNGKHOANDataSet1.LENHDAT);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực thi SP!\n" + ex.Message, "Thông báo",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (Program.myReader == null) return;
            Program.myReader.Read();
            Program.conn.Close();
        }

        private void txtMaCP_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar.ToString().IndexOfAny(@"0123456789!@#$%^&*()_+=|\{}[]?><.,';:".ToCharArray()) != -1)  // chổ nầy ko hiểu tại sao lại !=-1
            {
                e.Handled = true;
                MessageBox.Show("Mã cổ phiếu phải nhập kiểu chữ và không có ký tự đặt biệt", "Thông báo");
                return;
            }
            else
                e.Handled = false;
        }

        private void txtSoLuong_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8)
            {
                e.Handled = true;
                MessageBox.Show("Số lượng sai định dạng, vui lòng chỉ nhập số", "Thông báo");
                return;
            }
            else
                e.Handled = false;
        }

        private void txtGiaDat_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8)
            {
                e.Handled = true;
                MessageBox.Show("Giá sai định dạng, vui lòng chỉ nhập số", "Thông báo");
                return;
            }
            else
                e.Handled = false;
        }
    }
}
