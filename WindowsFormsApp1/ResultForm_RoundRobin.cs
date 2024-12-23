﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ResultForm_RoundRobin : Form
    {
        public ResultForm_RoundRobin()
        {
            InitializeComponent();
        }
        private List<Process_Class> processes;
        private List<GanttEvent> ganttEvents = new List<GanttEvent>();
        public ResultForm_RoundRobin(List<Process_Class> processes, int quantum)
        {
            InitializeComponent();
            this.processes = processes;
            CalculateRoundRobin(quantum,ganttEvents);
            DrawGanttChartTable(ganttEvents);
            PopulateResultTable();
        }

        private void CalculateRoundRobin(int quantum, List<GanttEvent> ganttEvents)
        {
            ganttEvents.Clear();
            List<Process_Class> processesCopy = processes.Select(p => new Process_Class
            {
                ArrivalTime = p.ArrivalTime,
                BurstTime = p.BurstTime,
                RemainingTime = p.BurstTime
            }).ToList();

            int currentTime = 0;
            List<Process_Class> completedProcesses = new List<Process_Class>();
            Queue<Process_Class> readyQueue = new Queue<Process_Class>();
            Process_Class currentProcess = null;
            int lastStartTime = 0;

            while (processesCopy.Count > 0 || readyQueue.Count > 0)
            {
                // Thêm process đến vào hàng đợi
                foreach (var process in processesCopy.Where(p => p.ArrivalTime <= currentTime).ToList())
                {
                    readyQueue.Enqueue(process);
                    processesCopy.Remove(process);
                }


                if (readyQueue.Count == 0)
                {
                    currentTime++;
                    continue;
                }

                currentProcess = readyQueue.Dequeue();

                if (currentProcess.RemainingTime == currentProcess.BurstTime)
                {
                    currentProcess.ResponseTime = currentTime - currentProcess.ArrivalTime;
                }

                int executionTime = Math.Min(quantum, currentProcess.RemainingTime);
                ganttEvents.Add(new GanttEvent
                {
                    StartTime = currentTime,
                    EndTime = currentTime + executionTime,
                    ProcessId = currentProcess.ArrivalTime
                });

                currentProcess.RemainingTime -= executionTime;
                currentTime += executionTime;

                if (currentProcess.RemainingTime > 0)
                {
                    readyQueue.Enqueue(currentProcess);
                }
                else
                {
                    currentProcess.CompletionTime = currentTime;
                    currentProcess.WaitingTime = currentProcess.CompletionTime - currentProcess.ArrivalTime - currentProcess.BurstTime;
                    completedProcesses.Add(currentProcess);

                }
            }
            // Cập nhật lại các giá trị hoàn thành, phản hồi, chờ cho process ban đầu
            foreach (var process in processes)
            {
                var completedProcess = completedProcesses.FirstOrDefault(cp => cp.ArrivalTime == process.ArrivalTime && cp.BurstTime == process.BurstTime);
                if (completedProcess != null)
                {
                    process.CompletionTime = completedProcess.CompletionTime;
                    process.WaitingTime = completedProcess.WaitingTime;
                    process.ResponseTime = completedProcess.ResponseTime;
                }
            }
        }
        private void DrawGanttChartTable(List<GanttEvent> ganttEvents)
        {
            // Xóa các cột cũ nếu có
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();


            if (ganttEvents.Count == 0) return;
            int totalTime = ganttEvents.Last().EndTime;
            float ganttWidth = dataGridView1.Width;

            //Thêm cột
            for (int i = 0; i < ganttEvents.Count; i++)
            {
                dataGridView1.Columns.Add($"C{i}", $"C{i}");
                dataGridView1.Columns[i].Width = (int)((float)(ganttEvents[i].EndTime - ganttEvents[i].StartTime) / totalTime * ganttWidth);

            }
            //Thêm một dòng
            dataGridView1.Rows.Add();

            // Tô màu các ô
            for (int i = 0; i < ganttEvents.Count; i++)
            {
                dataGridView1.Rows[0].Cells[i].Value = $"{ganttEvents[i].StartTime}-{ganttEvents[i].EndTime}";
                dataGridView1.Rows[0].Cells[i].Style.BackColor = GetColorForProcess(ganttEvents[i].ProcessId); // Hàm này để lấy màu
            }

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Để các cột tự động lấp đầy không gian

            // Ngăn người dùng sửa dữ liệu
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.ColumnHeadersVisible = true;
            dataGridView1.RowHeadersVisible = true;
        }

        // Hàm để tạo màu khác nhau cho mỗi tiến trình
        private Color GetColorForProcess(int processId)
        {
            // Bạn có thể tùy chỉnh cách tạo màu, ở đây tôi dùng một số màu cơ bản
            switch (processId % 5)
            {
                case 0: return Color.LightBlue;
                case 1: return Color.LightGreen;
                case 2: return Color.LightCoral;
                case 3: return Color.LightGoldenrodYellow;
                case 4: return Color.LightPink;
                default: return Color.White;
            }
        }


        private void PopulateResultTable()
        {
            // Xóa dữ liệu cũ nếu có trong DataGridView
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();

            // Thêm các cột vào DataGridView
            dataGridView2.Columns.Add("ArrivalTime", "Arrival Time");
            dataGridView2.Columns.Add("CompletionTime", "Completion Time");
            dataGridView2.Columns.Add("WaitingTime", "Waiting Time");
            dataGridView2.Columns.Add("ResponseTime", "Response Time");

            int totalWaitingTime = 0, totalResponseTime = 0; int totalCompleteTime = 0;

            foreach (var process in processes)
            {
                dataGridView2.Rows.Add(process.ArrivalTime, process.CompletionTime, process.WaitingTime, process.ResponseTime);
                totalWaitingTime += process.WaitingTime;
                totalResponseTime += process.ResponseTime;
                totalCompleteTime += process.CompletionTime;
            }

            // Thêm dòng tính trung bình
            int n = processes.Count;
            dataGridView2.Rows.Add("Average", (float)totalCompleteTime / n, (float)totalWaitingTime / n, (float)totalResponseTime / n);
        }
    }
}