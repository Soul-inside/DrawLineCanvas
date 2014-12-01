// Last Change: 2014 11 27 16:01

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DrawLineCanvas
{
	/// <summary>
	///     Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		/// <summary>
		///     Первая линия
		/// </summary>
		private bool _firstLine = true;

		/// <summary>
		///     Режим рисавания линии
		/// </summary>
		private bool _drawingMode;

		/// <summary>
		///     Временная линия
		/// </summary>
		private Line _tempLine;

		/// <summary>
		///     Вертикальная линия
		/// </summary>
		private bool _isVertical = true;

		/// <summary>
		///     Предыдущая координата X
		/// </summary>
		private double _prevX;

		/// <summary>
		///     Предыдущая координата Y
		/// </summary>
		private double _prevY;

		/// <summary>
		///     Новая координата X
		/// </summary>
		private double _newX;

		/// <summary>
		///     Новая координата Y
		/// </summary>
		private double _newY;

		/// <summary>
		///     Список всех нарисованных линий
		/// </summary>
		private readonly List<LineInfo> _linesList = new List<LineInfo>();

		/// <summary>
		///     Время открытия файла
		/// </summary>
		private DateTime _time;

		/// <summary>
		///     Врежим рисавания квадратов для выделения области
		/// </summary>
		private bool _drawRectangle;

		/// <summary>
		///     Первая точка на экране, с которой начинается выделение прямоугольника (Левая верхняя)
		/// </summary>
		private static Point _firstPoint;

		/// <summary>
		///     Вторая точка на экране, с которой продолжается выделение прямоугольника (Правая нижняя)
		/// </summary>
		private static Point _secondPoint;

		/// <summary>
		///     Прямоугольник для ручного выделения области
		/// </summary>
		private static Rectangle _rect = new Rectangle {Stroke = Brushes.Black, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top};

		/// <summary>
		///     Описывает ширину, высоту и расположение прямоугольника целого числа
		/// </summary>
		private static Int32Rect _rectForDraw;

		public MainWindow()
		{
			InitializeComponent();
			_drawRectangle = false;
		}

		/// <summary>
		///     Канвас подгоняем под размер изображения
		/// </summary>
		private void ResizeImage()
		{
			int kx, ky;
			if (ImgWell.Height > ScViewer.Height)
			{
				ky = Convert.ToInt32(ImgWell.Source.Height/ScViewer.Height);
				ImgWell.Height = ky * ImgWell.Height;
				if (ImgWell.Source.Width > ScViewer.Width )
				{
					kx = Convert.ToInt32(ImgWell.Source.Width  / ScViewer.Width );
					ImgWell.Width = kx * ImgWell.Width;
				}
			}
			CnvDraw.Height = ImgWell.Source.Height;
			CnvDraw.Width = ImgWell.Source.Width;
			
		}

		/// <summary>
		///     Повели мышкой по канвасу
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseMove(object sender, MouseEventArgs e)
		{
			// Определение правой нижней координаты временного прямоугольника
			if (_drawRectangle && e.LeftButton == MouseButtonState.Pressed)
			{
				_secondPoint = e.GetPosition(CnvDraw);
				MoveDrawRectangle(_firstPoint, _secondPoint);
			}
			else
			{
				// рисуем временную линию
				if (e.LeftButton == MouseButtonState.Pressed && (DateTime.Now - _time).TotalMilliseconds > 100)
				{
					_drawingMode = true;
					CnvDraw.Children.Remove(_tempLine);
					// первая линия
					if (_firstLine)
					{
						_prevX = e.GetPosition(CnvDraw).X;
						_prevY = _firstPoint.Y;
						_newX = _prevX;
						_newY = e.GetPosition(CnvDraw).Y;
					}
					else
					{
						// вертикальная линия
						if (_isVertical)
						{
							_newX = _prevX;
							_newY = e.GetPosition(CnvDraw).Y;
						}
							// горизонтальная линия
						else
						{
							_newX = e.GetPosition(CnvDraw).X;
							_newY = _prevY;
						}
					}
					_tempLine = new Line
					{
						X1 = _prevX,
						Y1 = _prevY,
						X2 = _newX,
						Y2 = _newY,
						Stroke = Brushes.DarkCyan
					};
					CnvDraw.Children.Add(_tempLine);
				}
					// если левая кнопка не нажата, и мы были в режиме рисования линий
				else if (_drawingMode)
				{
					_drawingMode = false;
					if (_firstLine)
					{
						_firstLine = false;
					}
					// создаем новую линию взамен временной
					var line = new Line
					{
						X1 = _prevX,
						Y1 = _prevY,
						X2 = _newX,
						Y2 = _newY,
						Stroke = Brushes.DarkBlue
					};
					line.StrokeThickness = 2;
					CnvDraw.Children.Add(line);
					CnvDraw.Children.Remove(_tempLine);
					_linesList.Add(new LineInfo(_prevX, _prevY, line));
					// определение координат линии
					if (Math.Abs(_firstPoint.X) < 0.02)
					{
						// если область определения не была задана
						MessageBox.Show("Задайти область определения!", "Внимание");
						RemoveLastLine();
					}
					// если координаты области определения не были указаны
					else
					{
						if (TbXmin.Text.Equals("") || TbXmax.Text.Equals("") || TbYmin.Text.Equals("") || TbYmax.Text.Equals(""))
						{
							MessageBox.Show("Вы не ввели все необходимые данные!", "Внимание");
						}
						// если после целой части введен неверный символ
						else
						{
							var ci = new CultureInfo("en-US") { NumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." } };
							try
							{
								var xmin = Convert.ToDouble(TbXmin.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								var xmax = Convert.ToDouble(TbXmax.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								var ymin = Convert.ToDouble(TbYmin.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								var ymax = Convert.ToDouble(TbYmax.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								X.Content = (Math.Abs((_prevX - _firstPoint.X) / (_rect.Width) * (xmax - xmin) + xmin).ToString("N2"))
									+ " ; " + (Math.Abs((_prevY - _firstPoint.Y) / (_rect.Height) * (ymax - ymin) + ymin)).ToString("N2");
								Y.Content = (Math.Abs((_newX - _firstPoint.X) / (_rect.Width) * (xmax - xmin) + xmin).ToString("N"))
									+ " ; " + (Math.Abs((_newY - _firstPoint.Y) / (_rect.Height) * (ymax - ymin) + ymin).ToString("N"));
								var fs = new FileStream("output.txt", FileMode.Append);
								var sw = new StreamWriter(fs);
								sw.WriteLine("Начальная координата линии:" + X.Content + "\t" + "Конечная координата линии:" + Y.Content);
								sw.Close();
								fs.Close();
							}
								// при вводе неккоредных символов в поле координат
							catch (FormatException)
							{
								MessageBox.Show("вы ввели некорректные данные!", "Внимание");
							}
						}
					}
					// запоминаем координаты
					_prevX = _newX;
					_prevY = _newY;
					// выходим из режима рисования линий
					_isVertical = !_isVertical;
				}
			}
		}

		/// <summary>
		///     Удаляет последнюю линию по backspace
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back && !_drawingMode && _linesList.Count > 0)
			{
				RemoveLastLine();
			}
		}

		/// <summary>
		/// Удаление последней нарисованной линии
		/// </summary>
		private void RemoveLastLine()
		{
			int lastIndex = _linesList.Count - 1;
			if (_linesList.Count >= 2)
			{
				_prevX = _linesList[lastIndex].StartX;
				_prevY = _linesList[lastIndex].StartY;
			}
			else
			{
				_prevY = 0;
				_firstLine = true;
			}
			CnvDraw.Children.Remove(_linesList[lastIndex].LineReference);
			_linesList.RemoveAt(_linesList.Count - 1);
			_isVertical = !_isVertical;
		}

		/// <summary>
		///     Загрузка изображения
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtImageLoad_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog
			{
				FileName = " Image ",
				DefaultExt = ".jpg",
				Filter = " Image (.jpg)|*.jpg"
			};
			bool? result = dlg.ShowDialog();
			if (result == true)
			{
				try
				{
					string filename = dlg.FileName;
					var bi3 = new BitmapImage();
					bi3.BeginInit();
					bi3.UriSource = new Uri(filename, UriKind.Absolute);
					bi3.EndInit();
					ImgWell.Source = bi3;
					_time = DateTime.Now;
					ResizeImage();
				}
				catch (NotSupportedException)
				{
					MessageBox.Show("Некорректный тип файла!", "Внимание");
				}
			}
		}

		/// <summary>
		///     Задание области определения функции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BtSelectArea_Click(object sender, RoutedEventArgs e)
		{
			CnvDraw.Children.Remove(_rect);
			while (_linesList.Count != 0)
			{
				RemoveLastLine();
			}
			_rect = new Rectangle {Stroke = Brushes.Red, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top};
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			_drawRectangle = true;
			CnvDraw.Children.Add(_rect);
		}

		/// <summary>
		///     Нажатие левой кнопки мыши на канвасе содержащем изображение
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_drawRectangle)
			{
				_firstPoint = e.GetPosition(CnvDraw);
				MoveDrawRectangle(_firstPoint, _firstPoint);
			}
		}

		/// <summary>
		///     Отпускание левой кнопки мыши на канвасе, содержащем изображение всего экрана.
		/// Построение постоянного квадрата
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CnvDraw_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_drawRectangle)
			{
				var rectangle = new Int32Rect();
				if (_firstPoint.X > _secondPoint.X)
				{
					rectangle.X = (int) (_secondPoint.X + _rect.StrokeThickness);
					rectangle.Width = (int) (_firstPoint.X - _secondPoint.X - _rect.StrokeThickness*2);
					if (rectangle.Width < 1)
					{
						rectangle.Width = 0;
					}
				}
				else
				{
					rectangle.X = (int) (_firstPoint.X);
					rectangle.Width = (int) (_secondPoint.X - _firstPoint.X);
					if (rectangle.Width < 1)
					{
						rectangle.Width = 0;
					}
				}

				if (_firstPoint.Y > _secondPoint.Y)
				{
					rectangle.Y = (int) (_secondPoint.Y + _rect.StrokeThickness);
					rectangle.Height = (int) (_firstPoint.Y - _secondPoint.Y - _rect.StrokeThickness*2);
					if (rectangle.Height < 1)
					{
						rectangle.Height = 0;
					}
				}
				else
				{
					rectangle.Y = (int) (_firstPoint.Y + _rect.StrokeThickness);
					rectangle.Height = (int) (_secondPoint.Y - _firstPoint.Y - _rect.StrokeThickness*2);
					if (rectangle.Height < 1)
					{
						rectangle.Height = 0;
					}
				}
				_drawRectangle = false;
			}
		}

		/// <summary>
		///     Растянуть/Переместить прямоугольник на указанные координаты. Построение временного квадрата.
		/// </summary>
		/// <param name="pointLeftTop">Левый верхний угол</param>
		/// <param name="pointRightBottom">Правый нижний угол</param>
		private void MoveDrawRectangle(Point pointLeftTop, Point pointRightBottom)
		{
			if (_drawRectangle)
			{
				_rectForDraw = new Int32Rect();
				if (pointLeftTop.X > pointRightBottom.X)
				{
					Canvas.SetLeft(_rect, pointRightBottom.X);
					_rect.Width = pointLeftTop.X - pointRightBottom.X;
					_rectForDraw.X = (int) pointRightBottom.X;
				}
				else
				{
					Canvas.SetLeft(_rect, pointLeftTop.X);
					_rect.Width = pointRightBottom.X - pointLeftTop.X;
					_rectForDraw.X = (int) pointLeftTop.X;
				}

				if (pointLeftTop.Y > pointRightBottom.Y)
				{
					Canvas.SetTop(_rect, pointRightBottom.Y);
					_rect.Height = pointLeftTop.Y - pointRightBottom.Y;
					_rectForDraw.Y = (int) pointRightBottom.Y;
				}
				else
				{
					Canvas.SetTop(_rect, pointLeftTop.Y);
					_rect.Height = pointRightBottom.Y - pointLeftTop.Y;
					_rectForDraw.Y = (int) pointLeftTop.Y;
				}
				_rectForDraw.Width = (int) _rect.Width;
				_rectForDraw.Height = (int) _rect.Height;
			}
		}

		private void BtGreen_Click(object sender, RoutedEventArgs e)
		{
			_firstLine = true;
		}

		private void TgBtOrange_Checked(object sender, RoutedEventArgs e)
		{
			_rect = new Rectangle { Stroke = Brushes.Red, StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Fill = Brushes.Red};
			Canvas.SetTop(_rect, _firstPoint.Y);
			Canvas.SetLeft(_rect, _firstPoint.X);
			_drawRectangle = true;
			CnvDraw.Children.Add(_rect);
		}

		private void BtRed_Click(object sender, RoutedEventArgs e)
		{
			_firstLine = true;
		}
	}
}
