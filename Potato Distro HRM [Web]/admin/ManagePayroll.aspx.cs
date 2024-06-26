﻿using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Potato_Distro_HRM__Web_.admin
{
    public partial class ManagePayroll : System.Web.UI.Page
    {

        Dictionary<int, int> idLeaveDays = new Dictionary<int, int>();
        DataTable dataTable;
        NpgsqlCommand mainCmd = new NpgsqlCommand("SELECT employee.id, (fname || ' ' || lname) as name, department.name as dept, salary, leaves.start as leave_start, leaves.days as leave_days " +
                    "FROM (employee LEFT OUTER JOIN department ON department.id = employee.dept) LEFT OUTER JOIN " +
                    "( SELECT * FROM leave WHERE status = 2 ) AS leaves " +
                    "ON employee.id = leaves.empid " +
                    "ORDER BY employee.id");
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                monthLbl.Text = "For the month of " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " + DateTime.Now.Year;
                ViewState["currentMonth"] = DateTime.Now.Month;
                ViewState["currentYear"] = DateTime.Now.Year;

                NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["potato_dbConnectionString"].ConnectionString);
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter("SELECT id, name FROM department", conn);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                DataRow datarow = dataTable.NewRow();
                datarow.ItemArray = new object[] { 0, "Department" };
                dataTable.Rows.InsertAt(datarow, 0);

                DeptDdl.DataSource = dataTable;
                DeptDdl.DataTextField = "name";
                DeptDdl.DataValueField = "id";
                DeptDdl.DataBind();

                
                
                GetNewQuery(mainCmd);
            }
            
        }

        private void GetNewQuery(NpgsqlCommand cmd)
        {
            NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["potato_dbConnectionString"].ConnectionString);
            cmd.Connection = conn;
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);

            dataTable = new DataTable();
            adapter.Fill(dataTable);
            UpdateListView();
        }

        private void UpdateListView()
        {
            int currentYear = (int)ViewState["currentYear"];
            int currentMonth = (int)ViewState["currentMonth"];
            DataTable newDataTable = new DataTable();

            DataColumn column = new DataColumn("id", typeof(int));
            newDataTable.Columns.Add(column);

            column = new DataColumn("name", typeof(string));
            newDataTable.Columns.Add(column);

            column = new DataColumn("dept", typeof(string));
            newDataTable.Columns.Add(column);

            column = new DataColumn("salary", typeof(double));
            newDataTable.Columns.Add(column);

            column = new DataColumn("leave_start", typeof(DateTime));
            newDataTable.Columns.Add(column);

            column = new DataColumn("leave_days", typeof(int));
            newDataTable.Columns.Add(column);

            column = new DataColumn("counted_leave_days", typeof(int));
            newDataTable.Columns.Add(column);

            DataRow datarow;

            foreach(DataRow row in dataTable.Rows)
            {
                int countedLeaveDays = 0;

                if (row["leave_start"] != DBNull.Value)
                {
                    DateTime startDate = Convert.ToDateTime(row["leave_start"]);
                    int days = Convert.ToInt32(row["leave_days"]);
                    DateTime endDate = startDate.AddDays(days);
                    int startMonth = startDate.Month;
                    int endMonth = endDate.Month;

                    if (startMonth < currentMonth && endMonth > currentMonth)
                    {
                        countedLeaveDays = DateTime.DaysInMonth(currentYear, currentMonth);
                    }
                    else if (startMonth == currentMonth)
                    {
                        if (endMonth > currentMonth)
                        {
                            countedLeaveDays = DateTime.DaysInMonth(currentYear, currentMonth) - startDate.Day;
                        }
                        else if (endMonth == currentMonth)
                        {
                            countedLeaveDays = endDate.Day - startDate.Day;
                        }
                    }
                    else if (endMonth == currentMonth)
                    {
                        countedLeaveDays = endDate.Day;
                    }
                }

                
                datarow = newDataTable.NewRow();
                datarow.ItemArray = new object[] { row["id"], row["name"], row["dept"], row["salary"], row["leave_start"], row["leave_days"], countedLeaveDays };
                newDataTable.Rows.Add(datarow);
            }

            

            var totalLeaveDays =
                from entry in newDataTable.AsEnumerable()
                group entry by entry.Field<int>("id") into grp
                select new
                {
                    id = grp.Key,
                    total_leave_days = grp.Sum(r => r.Field<int>("counted_leave_days"))
                };

            var empinfo = 
                (from entry in newDataTable.AsEnumerable()
                select new
                {
                    id = entry.Field<int>("id"),
                    name = entry.Field<string>("name"),
                    dept = entry.Field<string>("dept"),
                    salary = entry.Field<double>("salary")
                }).Distinct();


            var all =
                (from leave in totalLeaveDays
                join emp in empinfo on leave.id equals emp.id
                select new
                {
                    id = leave.id,
                    name = emp.name,
                    dept = emp.dept,
                    salary = emp.salary,
                    total_leave_days = leave.total_leave_days
                }).Distinct();

            ListView1.DataSource = all;
            ListView1.DataBind();
        }

        protected void ListView1_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            int currentYear = (int)ViewState["currentYear"];
            int currentMonth = (int)ViewState["currentMonth"];
            ListViewDataItem dataItem = (ListViewDataItem)e.Item;

            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                dynamic rowView = dataItem.DataItem as dynamic;
                int totalLeaveDays = Convert.ToInt32(rowView.total_leave_days);
                int monthlySalary = Convert.ToInt32(rowView.salary);
                Label salaryLbl = (Label)e.Item.FindControl("salary");
                salaryLbl.Text = (Math.Round(monthlySalary - totalLeaveDays * (monthlySalary * 1.0 / DateTime.DaysInMonth(currentYear, currentMonth)), 2)).ToString();

                idLeaveDays.Add(Convert.ToInt32(rowView.id), totalLeaveDays);
            }
        }

        protected void detailsBtn_Click(object sender, EventArgs e)
        {
            Session["context"] = new int[] { (int)ViewState["currentYear"], (int)ViewState["currentMonth"] };
            LinkButton btn = (LinkButton)sender;
            string[] args = btn.CommandArgument.ToString().Split(',');
            Dictionary<int, int> one = new Dictionary<int, int>() { { Convert.ToInt32(args[0]), Convert.ToInt32(args[1]) } };

            Session["idLeaveDays"] = one;
            Response.Redirect("SalarySummary.aspx");
        }

        public class EmpSalaryDetails
        {
            public int id { get; set; }

            public int dept { get; set; }
            public int countedLeaveDays { get; set; }
            public int month { get; set; }
            public int year { get; set; }

            public EmpSalaryDetails(int id, int dept, int countedLeaveDays, int month, int year)
            {
                this.id = id;
                this.dept = dept;
                this.countedLeaveDays = countedLeaveDays;
                this.month = month;
                this.year = year;
            }

        }


        protected void monthBtn_Click(object sender, EventArgs e)
        {
            monthLbl.Text = "For the month of " + Request["date"];
            ViewState["currentMonth"] = DateTime.ParseExact(Request["date"], "MMMM yyyy", CultureInfo.CurrentCulture).Month;
            ViewState["currentYear"] = DateTime.ParseExact(Request["date"], "MMMM yyyy", CultureInfo.CurrentCulture).Year;
            GetNewQuery(mainCmd);

        }

        protected void filterBtn_Click(object sender, EventArgs e)
        {
            NpgsqlCommand cmd;

            int deptid = Convert.ToInt32(DeptDdl.SelectedValue);
            string empname = NameTb.Text.Trim();
            if (!string.IsNullOrEmpty(empname) && deptid != 0)
            {
                cmd = new NpgsqlCommand("SELECT employee.id, (fname || ' ' || lname) as name, department.name as dept, salary, leaves.start as leave_start, leaves.days as leave_days " +
                    "FROM (employee LEFT OUTER JOIN department ON department.id = employee.dept) LEFT OUTER JOIN " +
                    "( SELECT * FROM leave WHERE status = 2 ) AS leaves " +
                    "ON employee.id = leaves.empid " +
                    "WHERE (fname || ' ' || lname) LIKE ('%' || :name || '%') AND department.id=:dept " +
                    "ORDER BY employee.id");
                cmd.Parameters.Add(new NpgsqlParameter("name", empname));
                cmd.Parameters.Add(new NpgsqlParameter("dept", deptid));

            }
            else if (!string.IsNullOrEmpty(empname))
            {
                cmd = new NpgsqlCommand("SELECT employee.id, (fname || ' ' || lname) as name, department.name as dept, salary, leaves.start as leave_start, leaves.days as leave_days " +
                    "FROM (employee LEFT OUTER JOIN department ON department.id = employee.dept) LEFT OUTER JOIN " +
                    "( SELECT * FROM leave WHERE status = 2 ) AS leaves " +
                    "ON employee.id = leaves.empid " +
                    "WHERE (fname || ' ' || lname) LIKE ('%' || :name || '%') " +
                    "ORDER BY employee.id");
                cmd.Parameters.Add(new NpgsqlParameter("name", empname));
            }
            else if (deptid != 0)
            {
                cmd = new NpgsqlCommand("SELECT employee.id, (fname || ' ' || lname) as name, department.name as dept, salary, leaves.start as leave_start, leaves.days as leave_days " +
                    "FROM (employee LEFT OUTER JOIN department ON department.id = employee.dept) LEFT OUTER JOIN " +
                    "( SELECT * FROM leave WHERE status = 2 ) AS leaves " +
                    "ON employee.id = leaves.empid " +
                    "WHERE department.id=:dept " +
                    "ORDER BY employee.id");
                cmd.Parameters.Add(new NpgsqlParameter("dept", deptid));
            }
            else
            {
                GetNewQuery(mainCmd);
                return;
            }            
            GetNewQuery(cmd);
        }

        protected void clearFilterBtn_Click(object sender, EventArgs e)
        {
            GetNewQuery(mainCmd);
        }


        protected void printAllBtn_Click(object sender, EventArgs e)
        {
            Session["context"] = new int[] { (int)ViewState["currentYear"], (int)ViewState["currentMonth"] };

            Session["idLeaveDays"] = (Dictionary<int,int>)ViewState["idLeaveDays"];
            Response.Redirect("SalarySummary.aspx");

            //ClientScript.RegisterStartupScript(this.GetType(), "onclick", "<script language=javascript>window.open('SalarySummary.aspx','PrintMe','height=300px,width=300px,scrollbars=1');</script>");
        }

        protected void ListView1_DataBound(object sender, EventArgs e)
        {
            ViewState["idLeaveDays"] = idLeaveDays;
        }
    }
}