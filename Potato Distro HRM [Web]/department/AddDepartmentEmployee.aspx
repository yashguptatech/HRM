﻿<%@ Page Title="" Language="C#" MasterPageFile="~/template/Admin.Master" AutoEventWireup="true" CodeBehind="AddDepartmentEmployee.aspx.cs" Inherits="Potato_Distro_HRM__Web_.department.AddDepartmentEmployee" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div id="add-form">
        <hr /><div class="bold h2">Department Employee Details</div><hr />
        <asp:Panel ID="deptMessagePanel" runat="server" Visible="false">
            <a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a>
            <asp:Label ID="deptMessageLabel" runat="server" />
        </asp:Panel>
    </div>
    <br />
    <div class="text-center">
        <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="btn btn-danger btn-lg" OnClick="btnBack_Click" CausesValidation="false"/>
    </div>
    <br /><br />
    <div id="accordion">
        <h3>Search</h3>
        <div>
            <div class="row">
                <div class="col-md-2 my-auto">
                    <asp:Label runat="server" Text="Search Input" />
                </div>
                <div class="col-md-5 my-auto">
                    <asp:TextBox ID="searchBox" runat="server" placeholder="Type search term here..." CssClass="form-control"/>
                </div>
                <div class="col-md-1 my-auto">
                    <button runat="server" id="btnSearch" class="btn btn-primary" onserverclick="SearchBtn_OnClick">
                        <i class="fas fa-search" style="color:white"></i>
                    </button> 
                    <asp:LinkButton ID="btnClearSearch" runat="server" CssClass="btn btn-dark" ToolTip="Clear Searches" Enabled="False" OnClick="ClearSearchBtn_onClick">
                        <i class="far fa-trash-alt" style="color:white"></i>
                    </asp:LinkButton>
                </div>
                <div class="col-md-1 my-auto">
                    <div class="text-truncate">Search by</div>
                </div>
                <div class="col-md-2 my-auto">
                    <asp:DropDownList ID="searchCriteria" runat="server" CssClass="form-control">
                        <asp:ListItem Text="First name" Value="1" />
                        <asp:ListItem Text="Last name" Value="2" />
                    </asp:DropDownList>
                </div>
            </div>
        </div>
    </div>
    <asp:Panel ID="zeroResultPanel" runat="server" CssClass="text-center" Visible="false"><span class="h4 text-secondary">No free employee found.</span></asp:Panel>
    <div id="gridview">
        <asp:GridView ID="employeeGridView" runat="server" AutoGenerateColumns="False" CellPadding="4" ForeColor="#333333" GridLines="None" Width="100%" DataKeyNames="id"  OnDataBound="EmployeeGridView_DataBound" AllowSorting="True" OnSorting="employeeGridView_Sorting">
            <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
            <Columns>
                <asp:TemplateField HeaderText="ID" InsertVisible="False" SortExpression="id" >
                    <ItemTemplate>
                        <asp:Label ID="empId" runat="server" Text='<%# Bind("id") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="First Name" SortExpression="fname">
                    <ItemTemplate>
                        <asp:Label ID="fname" runat="server" Text='<%# Bind("fname") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Last Name" SortExpression="lname">
                    <ItemTemplate>
                        <asp:Label ID="lname" runat="server" Text='<%# Bind("lname") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton ID="addEmpBtn" runat="server" CssClass="btn btn-success btn-sm" CommandArgument='<%#Eval("id") %>' OnCommand="AddDeptEmployee">
                            <i class="fas fa-user-edit"></i>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EditRowStyle BackColor="#999999" />
            <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
            <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
            <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
            <SortedAscendingCellStyle BackColor="#E9E7E2" />
            <SortedAscendingHeaderStyle BackColor="#506C8C" />
            <SortedDescendingCellStyle BackColor="#FFFDF8" />
            <SortedDescendingHeaderStyle BackColor="#6F8DAE" />
        </asp:GridView>
    </div>

    <script>
        $(document).ready(function () {
            $(function () {
                $("#accordion").accordion({
                    heightStyle: "content"
                });
            });

            $('#<%=searchBox.ClientID%>').keypress(function (event) {
                var keycode = (event.keyCode ? event.keyCode : event.which);
                if (keycode == '13') {
                    $("#btnSearch").click();
                }
                return true;
            });
        });
    </script>
</asp:Content>
