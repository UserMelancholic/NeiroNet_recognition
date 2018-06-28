using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeiroNet1
{
    public partial class MainForm : Form
    {
        private NeiroWeb nw;
        private Point startP;
        private int[,] arr;

        private bool enableTraining;

        public MainForm()
        {
            InitializeComponent();
            enableTraining = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NeiroGraphUtils.ClearImage(pictureBox1);
            nw = new NeiroWeb();
            string[] items = nw.GetLiteras();
            if (items.Length > 0)
            {
                comboBox.Items.AddRange(items);
                comboBox.SelectedIndex = 0;
            }            
        }

      

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            nw.SaveState();
        }

         private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point endP = new Point(e.X,e.Y);
                Bitmap image = (Bitmap)pictureBox1.Image;
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.DrawLine(new Pen(Color.ForestGreen,7), startP, endP);
                }
                pictureBox1.Image = image;
                startP = endP;                
            }
        }    

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {    
            startP = new Point(e.X, e.Y);
        }     

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loadSymvolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addZipFile();
        }

        private void addZipFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Filter = "All files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            string[] ls = File.ReadAllLines(openFileDialog.FileName, Encoding.GetEncoding(1251));
            if (ls.Length == 0) return;
            var newItems = new List<string>();
            foreach (var i in comboBox.Items) if (!newItems.Contains((string)i)) newItems.Add((string)i);
            foreach (var i in             ls) if (!newItems.Contains((string)i)) newItems.Add((string)i);
            newItems.Sort();
            comboBox.Items.Clear();
            comboBox.Items.AddRange(newItems.ToArray());
            comboBox.SelectedIndex = 0;            
        }


        private void toMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string litera = comboBox.SelectedIndex >= 0 ? (string)comboBox.Items[comboBox.SelectedIndex] : comboBox.Text;
            if (litera.Length == 0)
            {
                MessageBox.Show("Не выбран ни один символ для занесения в память.");
                return;
            }
            nw.SetTraining(litera, arr);
            NeiroGraphUtils.ClearImage(pictureBox1);
            NeiroGraphUtils.ClearImage(pictureBox2);
            NeiroGraphUtils.ClearImage(pictureBox3);
            MessageBox.Show("Выбранный символ '" + litera + "' успешно добавлен в память сети");
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NeiroGraphUtils.ClearImage(pictureBox1);
            NeiroGraphUtils.ClearImage(pictureBox2);
            NeiroGraphUtils.ClearImage(pictureBox3);
            label4.Text = "";
        }

        private void learnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Learn();
        }

        private void drawFromComboBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NeiroGraphUtils.ClearImage(pictureBox1);
            NeiroGraphUtils.ClearImage(pictureBox2);
            NeiroGraphUtils.ClearImage(pictureBox3);
            pictureBox1.Image = NeiroGraphUtils.DrawLitera(pictureBox1.Image, (string)comboBox.SelectedItem);
        }

     

        private void Learn()
        {
            int[,] clipArr = NeiroGraphUtils.CutImageToArray((Bitmap)pictureBox1.Image, new Point(pictureBox1.Width, pictureBox1.Height));
            if (clipArr == null) return;
            arr = NeiroGraphUtils.LeadArray(clipArr, new int[NeiroWeb.neironInArrayWidth, NeiroWeb.neironInArrayHeight]);
            pictureBox2.Image = NeiroGraphUtils.GetBitmapFromArr(clipArr);
            pictureBox3.Image = NeiroGraphUtils.GetBitmapFromArr(arr);
            string s = nw.CheckLitera(arr);
            if (s == null) s = "null";
            label4.Text = s;
            DialogResult askResult = MessageBox.Show("Результат распознавания - " + s + " ?", "", MessageBoxButtons.YesNo);
            if ( askResult != DialogResult.Yes || !enableTraining || MessageBox.Show("Добавить этот образ в память нейрона '" + s + "'" , "", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            nw.SetTraining(s, arr);
            // очищаем рисунки
            NeiroGraphUtils.ClearImage(pictureBox1);
            NeiroGraphUtils.ClearImage(pictureBox2);
            NeiroGraphUtils.ClearImage(pictureBox3);
          }

        private void Write()
        {
            int[,] clipArr = NeiroGraphUtils.CutImageToArray((Bitmap)pictureBox1.Image, new Point(pictureBox1.Width, pictureBox1.Height));
            if (clipArr == null) return;
            arr = NeiroGraphUtils.LeadArray(clipArr, new int[NeiroWeb.neironInArrayWidth, NeiroWeb.neironInArrayHeight]);
            pictureBox2.Image = NeiroGraphUtils.GetBitmapFromArr(clipArr);
            pictureBox3.Image = NeiroGraphUtils.GetBitmapFromArr(arr);
            string s = nw.CheckLitera(arr);
            if (s == null) s = "null";
            textBox2.Text += s;
            NeiroGraphUtils.ClearImage(pictureBox1);
            NeiroGraphUtils.ClearImage(pictureBox2);
            NeiroGraphUtils.ClearImage(pictureBox3);
            nw.SetTraining(s, arr);
            label4.Text = s;
        }

        // процедура помещает строку в список значений
        private void AddSymbolToList(string symbol)
        {
            if (symbol == null || symbol.Length == 0)
            {
                MessageBox.Show("Значение не может иметь длину 0 символов.");
                return;
            }
            comboBox.Items.Add(symbol);
            comboBox.SelectedIndex = comboBox.Items.Count - 1;
            MessageBox.Show("Сейчас значение '" + symbol + "' в списке, теперь можно научить нейросеть сеть его распознавать.");
        }

     

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) AddSymbolToList(((TextBox)sender).Text);
        }   

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (radioButton1.Checked)
            {
                label3.Visible = true;
                textBox1.Visible = true;
                button1.Visible = true;
                label7.Visible = true;
                pictureBox3.Visible = true;
                if (e.Button == MouseButtons.Right) Learn();
            }
            if(radioButton2.Checked)
            {
                label3.Visible = false;
                textBox1.Visible = false;
                button1.Visible = false;
                enableTraining = false;
                toMemoryToolStripMenuItem.Enabled = false;
                label7.Visible = false;
                pictureBox3.Visible = false;
                if (e.Button == MouseButtons.Right) Write();
            }
        }

        private void enableTrainingToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }


        private void button1_Click(object sender, EventArgs e)
        {
            enableTraining = true;
            toMemoryToolStripMenuItem.Enabled = true;
            AddSymbolToList(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName);
                streamWriter.WriteLine(textBox2.Text);
                streamWriter.Close();
            }
        }

        private void очистититьПолеСТекстомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void сохранитьДокументСТекстомToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName);
                streamWriter.WriteLine(textBox2.Text);
                streamWriter.Close();
            }
        }

        private void оПрограмеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Вас приветствует программа распознавания текста <<Буквица>>.                    С помощью неё вы можете распознать рукописный текст, а также научить программу распознавать новые символы.");
        }

        private void инструкцияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Для того, чтобы распознать текст или научить новому символу, выберите соотвествующий режим работы программы. В режиме обучения сначала добавьте необходимый символ в поле ввода, нарисуйте его и сохраните в памяти, затем обучите сеть.");
        }

        private void разработчикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Прогамму <<Буквица>> разработала компания <<N + B>>.                               Все права защищены.");
        }
    }
    
}
