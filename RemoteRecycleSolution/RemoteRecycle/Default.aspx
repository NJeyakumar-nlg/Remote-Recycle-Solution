<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <table>
        <tr>
            <td>
                Machine Name / IP Address :
            </td>
            <td>
                <asp:DropDownList ID="ddlMachineList" runat="server" Height="23px" Width="250px">
                </asp:DropDownList>
            </td>
            <td>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="ddlMachineList"
                    ErrorMessage="Select Server Name" InitialValue="Select"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td>
                Status Check?
            </td>
            <td>
                <asp:CheckBox ID="cbStatus" runat="server" />
            </td>
            <td>
            </td>
        </tr>
        <tr>
            <td>
                Message:
            </td>
            <td>
                <asp:Literal Text="" ID="litMessage" runat="server" />
            </td>
            <td>
            </td>
        </tr>
    </table>
    <br />
    <table>
        <tr>
            <td>
                <asp:Button ID="btnListAppPool" runat="server" Text="Show App Pools" OnClick="btnListAppPool_Click"
                    Width="145px" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:GridView ID="gvAppPool" runat="server" OnRowCommand="gvAppPool_RowCommand" AutoGenerateColumns="False"
                    BorderColor="#336699" BorderStyle="Solid" BorderWidth="2px" CellPadding="4" EmptyDataText="No data available."
                    DataKeyNames="AppPoolName">
                    <FooterStyle BackColor="#C6C3C6" ForeColor="Black" />
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField />
                        <asp:BoundField DataField="AppPoolName" HeaderText="AppPoolName" SortExpression="AppPoolName" />
                        <asp:BoundField />
                        <asp:BoundField DataField="Status" HeaderText="Status" SortExpression="Status" />
                        <asp:BoundField />
                        <asp:ButtonField ButtonType="button" CommandName="Recycle" HeaderText="Recycle" Text="Recycle" />
                    </Columns>
                </asp:GridView>
            </td>
        </tr>
    </table>
</asp:Content>
