﻿using LotusLabsTimeTracker.model;
using LotusLabsTimeTracker.controllers;
using LotusLabsTimeTracker.utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LotusLabsTimeTracker.views
{
    public partial class ProductionModule : Form
    {
        private MainMenu _mainMenu;
        private Users currentSessionUser;
        private int seconds = 0;
        private int minutes = 0;
        private int hours = 0;
        private DateTime startDate = DateTime.Now;
        private long runningId;
        private Timer myTimer = new Timer();


        public ProductionModule(MainMenu mainMenu)
        {
            InitializeComponent();
            this._mainMenu = mainMenu;
        }

        private void ProductionModule_Load(object sender, EventArgs e)
        {
            //Start timer 
            Timer timer = new Timer();
            timer.Interval = (1 * 1000); // 1 secs
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            lbl_empName.Text = currentSessionUser.firstName + " " + currentSessionUser.lastName;
            lbl_empNo.Text = currentSessionUser.username;
            lbl_currentDate.Text = DateTime.Now.ToString();

            foreach (model.WorkType workType in getLookupController().getWorkTypes(true))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Text = workType.name;
                item.Value = workType.id.ToString();
                cbo_prodWorkType.Items.Add(item);
            }

            foreach (model.TaskType taskType in getLookupController().getTaskTypes(true))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Text = taskType.name;
                item.Value = taskType.id.ToString();
                cbo_prodTaskType.Items.Add(item);
            }

        }

        private UserController getUserController()
        {
            return new UserController();
        }

        private StringUtility getStringUtility()
        {
            return new StringUtility();
        }

        public Users getCurrentSessionUser()
        {
            return this.currentSessionUser;
        }

        public void setCurrentSessionUser(Users currentSessionUser)
        {
            this.currentSessionUser = currentSessionUser;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            lbl_currentDate.Text = DateTime.Now.ToString();
        }

        private void ProductionModule_FormClosed(object sender, FormClosedEventArgs e)
        {
            _mainMenu.Show();
        }

        private LookupController getLookupController()
        {
            return new LookupController();
        }

        private void cbo_prodWorkType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBoxItem selectedItem = cbo_prodWorkType.SelectedItem as ComboBoxItem;
            model.WorkType workType = new model.WorkType();
            workType.id = selectedItem == null ? (long)0 : long.Parse(selectedItem.Value.ToString());
            
            cbo_prodProjType.Items.Clear();
            foreach (Project project in getLookupController().getProjectsByWorkType(true, workType))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Text = project.name;
                item.Value = project.id.ToString();
                cbo_prodProjType.Items.Add(item);
            }
        }

        private void btn_prodCancel_Click(object sender, EventArgs e)
        {
            _mainMenu.Show();
            this.Close();
        }

        private void counter_Tick(object sender, EventArgs e)
        {
            Invoke(new Action(() =>
                {
                    if (this.seconds < 59)
                    {
                        this.seconds++;
                    }
                    else
                    {
                        this.seconds = 0;
                        this.minutes++;
                    }

                    if (minutes == 59)
                    {
                        this.minutes = 0;
                        this.hours++;
                    }

                    lbl_elapsedTime.Text = hours.ToString().PadLeft(2, '0') + ":" +
                                           minutes.ToString().PadLeft(2, '0') + ":" +
                                           seconds.ToString().PadLeft(2, '0');
                }
            ));
        }

        private void btn_prodStart_Click(object sender, EventArgs e)
        {
            if (btn_prodStart.Text == "Start")
            {
                UserTask userTask = new UserTask();

                ComboBoxItem selectedWorkType = cbo_prodWorkType.SelectedItem as ComboBoxItem;
                userTask.workType = new model.WorkType();
                userTask.workType.id = selectedWorkType == null ? (long)0 : long.Parse(selectedWorkType.Value.ToString());

                ComboBoxItem selectedProject = cbo_prodProjType.SelectedItem as ComboBoxItem;
                userTask.project = new Project();
                userTask.project.id = selectedProject == null ? (long)0 : long.Parse(selectedProject.Value.ToString());

                ComboBoxItem selectedTaskType = cbo_prodTaskType.SelectedItem as ComboBoxItem;
                userTask.taskType = new model.TaskType();
                userTask.taskType.id = selectedTaskType == null ? (long)0 : long.Parse(selectedTaskType.Value.ToString());

                userTask.description = txt_prodDetails.Text;

                List<String> errMessages = getProductionController().validateUserTask(userTask);
                if (errMessages.Count > 0)
                {
                    MessageBox.Show(getStringUtility().arrayToStringMessages(errMessages));
                    return;
                }
                userTask.startDateTime = DateTime.Now;
                userTask.billable = chkBillable.Checked;
                userTask.user = this.currentSessionUser;
                userTask.activeFlag = true;

                UserTask result = getProductionController().saveUserTask(userTask, true, currentSessionUser);
                if (result.id != 0) {
                    btn_prodStart.Text = "Stop";
                    myTimer = new Timer();
                    myTimer.Interval = (1 * 1000); // 1 secs
                    myTimer.Tick += new EventHandler(counter_Tick);
                    myTimer.Start();
                    MessageBox.Show("Timer successfully started");
                    runningId = result.id;

                }
            }
            else {
                UserTask userTask = getProductionController().getUserTask(runningId);
                userTask.endDateTime = DateTime.Now;
                userTask.description = txt_prodDetails.Text;
                UserTask result = getProductionController().saveUserTask(userTask, false, currentSessionUser);
                if (result.id != 0) {
                    myTimer.Stop();
                    btn_prodStart.Text = "Start";
                    MessageBox.Show("Timer successfully stopped");
                }
            }

            cbo_prodProjType.Enabled = btn_prodStart.Text == "Start";
            cbo_prodTaskType.Enabled = btn_prodStart.Text == "Start";
            cbo_prodWorkType.Enabled = btn_prodStart.Text == "Start";

            if (btn_prodStart.Text == "Start") {
                clearFields();
            }
            
        }

        private void clearFields() {
            cbo_prodProjType.SelectedIndex = 0;
            cbo_prodTaskType.SelectedIndex = 0;
            cbo_prodWorkType.SelectedIndex = 0;
            this.seconds = 0;
            this.minutes = 0;
            this.hours = 0;
            lbl_elapsedTime.Text = "00:00:00";
            txt_prodDetails.Text = String.Empty;
        }

        private ProductionController getProductionController() {
            return new ProductionController();
        }
    }
}
