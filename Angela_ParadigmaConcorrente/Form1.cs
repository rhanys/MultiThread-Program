using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using ZedGraph;
using System.Drawing.Imaging;

namespace Angela_ParadigmaConcorrente
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        static int XSize = 200; //tamanho do da dimensao X do grafico
        static int YSize = 1; //tamanho do da dimensao Y do grafico
        Thread TWrite, TRead;
         double[] CircularBuffer = new double[XSize];
        Mutex MutexCircular, MutexAcesso;
         int p1, p2;
         
        RollingPointPairList ListaGrafico = new RollingPointPairList(XSize);
        int tempoX;
        int tempoExecução, contTempo; //conta tempo de execução do programa


        public Form1()
        {
            InitializeComponent();
        }

        double Sen_Funcao(double x)
        {
            double result = 0;

            result = (System.Math.Sin(x) * System.Math.Sin(2 * x));

            return result;
        }

         void Func1()
        {
            double x = 0;
            double point = 0;
            while (true)
            {
                MutexCircular.WaitOne();

                if (p1 != p2)
                {
                    point = Sen_Funcao(x);
                    CircularBuffer[p1] = point;
                    p1++;
                    x += .1;

                    if ( p1 == XSize)
                    {
                        p1 = 0;
                    }
                                          
                }
                MutexCircular.ReleaseMutex();
                Thread.Sleep(200);
            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tempoExecução > 0)
            {

                MutexAcesso.WaitOne();
                if (tempoX == XSize)
                {
                    tempoX = 0;
                    ListaGrafico.Clear();
                }
                zedGraph.GraphPane.AddCurve("", ListaGrafico, Color.Blue, SymbolType.None);
                zedGraph.Invalidate();
                contTempo++;


                if (contTempo == 5)
                {
                    tempoExecução--;
                    label2.Text = Convert.ToString(tempoExecução) + " Segundos";
                    contTempo = 0;
                }
                MutexAcesso.ReleaseMutex();
            }

            
            else
            {
                    TRead.Abort();
                    TWrite.Abort();
                    MutexCircular.Close();
                    MutexAcesso.Close();
                    buttonStop.Enabled = false;
                    buttonSaveGraph.Enabled = true;
                    timer1.Stop();
            } 



        }

         void Func2()
        {
            double Leitura;
            
            while (true)
            {
                MutexCircular.WaitOne();
                    p2++;
                    Leitura = CircularBuffer[p2];
                if (p2 == XSize -1)
                {
                    p2 = -1;
                }
                MutexAcesso.WaitOne();
                    ListaGrafico.Add(tempoX, Leitura);
                    tempoX++;
                    MutexAcesso.ReleaseMutex();
                MutexCircular.ReleaseMutex();
                Thread.Sleep(300);

            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            TRead.Abort();
            TWrite.Abort();
            timer1.Stop();
            MutexCircular.Close();
            MutexAcesso.Close();
            buttonStop.Enabled = false;
            buttonSaveGraph.Enabled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBoxTempo_TextChanged(object sender, EventArgs e)
        {
            buttonStart.Enabled = true;
        }

        private void buttonSaveGraph_Click(object sender, EventArgs e)
        {
            Bitmap printscreen = new Bitmap(this.Bounds.Width, this.Bounds.Height);
            Graphics graphics = Graphics.FromImage(printscreen);

            graphics.CopyFromScreen(this.Bounds.X, this.Bounds.Y, 0, 0, this.Bounds.Size);
            SaveFileDialog saveImageDialog = new SaveFileDialog();

            saveImageDialog.Title = "Seleccionar caminho para salvar:";
            saveImageDialog.Filter = "JPEG |*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|PNG Image|*.png|All files (*.*)|*.*";

            if (saveImageDialog.ShowDialog() == DialogResult.OK)
            {
                printscreen.Save(saveImageDialog.FileName, ImageFormat.Jpeg);
                buttonSaveGraph.Enabled = false;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            FormLoad();
            TWrite = new Thread(Func1);
            TRead = new Thread(Func2);
            MutexCircular = new Mutex();
            MutexAcesso = new Mutex();
            tempoX = 0;
            p1 = 0;
            p2 = -1;
            tempoExecução = Convert.ToInt32(textBoxTempo.Text);
            contTempo = 0;

            TWrite.Priority = ThreadPriority.AboveNormal;
            TRead.Priority = ThreadPriority.BelowNormal;

            ListaGrafico.Clear();
            zedGraph.GraphPane.AddCurve("", ListaGrafico, Color.Blue, SymbolType.None);
            buttonSaveGraph.Enabled = false;

            TWrite.Start();
            Thread.Sleep(500);
            TRead.Start();
            timer1.Start();


        }

        private void FormLoad()
        {
            zedGraph.GraphPane.Title.Text = "Sinal Gerado";
            zedGraph.GraphPane.XAxis.Title.Text = "Tempo";
            zedGraph.GraphPane.YAxis.Title.Text = "Amplitude";
            zedGraph.GraphPane.XAxis.Scale.Min = 0;
            zedGraph.GraphPane.XAxis.Scale.Max = XSize;
            zedGraph.GraphPane.XAxis.Scale.MinorStep = 1;
            zedGraph.GraphPane.XAxis.Scale.MajorStep = 5;
            zedGraph.GraphPane.YAxis.Scale.Min = -1;
            zedGraph.GraphPane.YAxis.Scale.Max = YSize;
            buttonStop.Enabled = true;

        }
    }
}
