using System.Text;
using System;
using System.Web;
using System.Web.UI;
using System.Management;
using System.DirectoryServices;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Configuration;
using System.Net.Mail;
using System.Collections.Specialized;


public partial class Default : System.Web.UI.Page
{
    static string logPath = string.Format("{0}/Logs/{1}", AppDomain.CurrentDomain.BaseDirectory, "Log" + DateTime.Now.ToLongDateString() + ".txt");

    protected global::System.Web.UI.WebControls.DropDownList ddlMachineList;
    protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator3;
    protected global::System.Web.UI.WebControls.CheckBox cbStatus;
    protected global::System.Web.UI.WebControls.Button btnListAppPool;
    protected global::System.Web.UI.WebControls.GridView gvAppPool;
    protected global::System.Web.UI.WebControls.Literal litMessage;

    DataTable dsAppPool;
    string appPoolPath;
    public string username;
    public string password;

    protected void Page_Load(object sender, EventArgs e)
    {
        litMessage.Text = "";
        username = ConfigurationManager.AppSettings["username"];
        password = ConfigurationManager.AppSettings["password"];

        if (!IsPostBack)
        {
            NameValueCollection servers = ((NameValueCollection)ConfigurationSettings.GetConfig("Servers"));
            ddlMachineList.Items.Add("Select");
            foreach (string name in servers)
            {
                ddlMachineList.Items.Add(new ListItem(name, servers[name]));
            }          
        }
    }

    protected void btnListAppPool_Click(object sender, EventArgs e)
    {
        if (ddlMachineList.SelectedValue != "")
        {
            appPoolPath = @"IIS://" + ddlMachineList.SelectedValue + "/W3SVC/AppPools";
            listdownAppPool();
            if (dsAppPool.Rows.Count > 0)
            {
                gvAppPool.Visible = true;
                gvAppPool.DataSource = dsAppPool;
                gvAppPool.DataMember = dsAppPool.Columns["AppPoolName"].ToString();
                gvAppPool.DataBind();
            }
        }
    }

    protected void gvAppPool_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        string currentCommand = e.CommandName;
        int currentRowIndex = Int32.Parse(e.CommandArgument.ToString());
        string appPoolName = gvAppPool.DataKeys[currentRowIndex].Value.ToString();

        if (currentCommand == "Recycle")
        {
            appPoolPath = @"IIS://" + ddlMachineList.SelectedValue + "/W3SVC/AppPools";
            DirectoryEntry w3svc = new DirectoryEntry(appPoolPath + "/" + appPoolName, username, password);
            w3svc = new DirectoryEntry(appPoolPath, username, password);
            foreach (DirectoryEntry appPool in w3svc.Children)
            {
                if (appPoolName.Equals(appPool.Name, StringComparison.OrdinalIgnoreCase))
                {
                    string strstatus = GetStatus(appPool);
                    if (strstatus != null)
                    {
                        if (strstatus == "Running")
                        {
                            try
                            {
                                object obj = appPool.Invoke("Recycle", null);
                                appPool.CommitChanges();
                                listdownAppPool();
                                litMessage.Text = "Recycled";

                                LogIt(appPool.Name, ddlMachineList.SelectedValue);

                                gvAppPool.Visible = false;
                                ddlMachineList.SelectedIndex = 0;

                                break;
                            }
                            catch (Exception ex)
                            {
                                Response.Write(ex.ToString());
                            }
                        }
                        else if (strstatus == "Stopped")
                        {
                            litMessage.Text = "App Pool is stopped";
                        }
                        else
                        {
                            litMessage.Text = "App Pool is having some issue.";
                        }
                    }
                }
            }
        }
    }

    private void LogIt(string appName, string machineId)
    {
        try
        {
            System.IO.StreamWriter objStreamWriter;

            using (objStreamWriter = new System.IO.StreamWriter(logPath, true))
            {
                objStreamWriter.WriteLine("=======================================" + Environment.NewLine);
                objStreamWriter.WriteLine("Time:" + System.DateTime.Now + Environment.NewLine);
                objStreamWriter.WriteLine("User:" + Context.User.Identity.Name + Environment.NewLine);
                objStreamWriter.WriteLine("AppName:" + appName + Environment.NewLine);
                objStreamWriter.WriteLine("MachineId:" + machineId + Environment.NewLine + Environment.NewLine);

                objStreamWriter.Close();
            }

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient( ConfigurationManager.AppSettings["SMTPServer"]);

                mail.From = new MailAddress(ConfigurationManager.AppSettings["fromId"]);
                mail.To.Add(ConfigurationManager.AppSettings["toId"]);
                mail.CC.Add(ConfigurationManager.AppSettings["ccId"]);
                mail.Priority = MailPriority.High;
                mail.Subject = ConfigurationManager.AppSettings["Subject"];
                mail.Body = "App Pool has been recycled : AppPool: " + appName + "; User: " + Context.User.Identity.Name + "; Machine Id: " + machineId;

                SmtpServer.Port = 25;
                SmtpServer.Credentials = new System.Net.NetworkCredential(username, password);

                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                using (objStreamWriter = new System.IO.StreamWriter(logPath, true))
                {
                    objStreamWriter.WriteLine("Error: While sending mail" + Environment.NewLine + Environment.NewLine);
                    objStreamWriter.Close();
                }
            }
        }
        catch (Exception)
        {
            litMessage.Text = "Recycled: Error while sending mail.";
        }
    }

    protected void listdownAppPool()
    {
        try
        {
            createDataTable();
            int count = 0;
            appPoolPath = @"IIS://" + ddlMachineList.SelectedValue + "/W3SVC/AppPools";
            DirectoryEntry w3svc = new DirectoryEntry(appPoolPath, username, password);
            foreach (DirectoryEntry appPool in w3svc.Children)
            {
                DataRow row = dsAppPool.NewRow();
                count = count + 1;

                row[0] = count;
                row[1] = appPool.Name;
                if (cbStatus.Checked == true)
                {
                    row[2] = GetStatus(appPool);
                }
                else
                {
                    row[2] = "NA";
                }


                dsAppPool.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            Response.Write(ex.ToString());
        }
    }
    protected string GetStatus(DirectoryEntry appPool)
    {
        int intStatus = 0;
        string status;
        intStatus = (int)appPool.InvokeGet("AppPoolState");
        switch (intStatus)
        {
            case 2:
                status = "Running";
                break;
            case 4:
                status = "Stopped";
                break;
            default:
                status = "Unknown";
                break;
        }
        return status;
    }
    protected void createDataTable()
    {
        dsAppPool = new DataTable();
        DataColumn col1 = new DataColumn("ID");
        DataColumn col2 = new DataColumn("AppPoolName");
        DataColumn col3 = new DataColumn("Status");

        col1.DataType = System.Type.GetType("System.Int32");
        col2.DataType = System.Type.GetType("System.String");
        col3.DataType = System.Type.GetType("System.String");

        dsAppPool.Columns.Add(col1);
        dsAppPool.Columns.Add(col2);
        dsAppPool.Columns.Add(col3);


    }

}
