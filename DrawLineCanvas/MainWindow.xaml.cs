﻿// Last Change: 2014 11 27 16:01

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
		///     Мы рисуем первую линию
		/// </summary>
		private bool _firstLine = true;

		/// <summary>
		///     В настоящий момент мы рисуем линию, а не просто елозим мышкой
		/// </summary>
		private bool _drawingMode;

		/// <summary>
		///     Временная линия
		/// </summary>
		private Line _tempLine;

		/// <summary>
		///     Рисуем вертикальную линию
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
		///     Выделение области определения функции
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
		///     Прямоугольная область для рисования не затенённой части изображения
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
			//режим задания области определения
			if (_drawRectangle && e.LeftButton == MouseButtonState.Pressed)
			{
				_secondPoint = e.GetPosition(CnvDraw);
				MoveDrawRectangle(_firstPoint, _secondPoint);
			}
			else
			{
				// если левая кнопка нажата, рисуем временную линию

				if (e.LeftButton == MouseButtonState.Pressed && (DateTime.Now - _time).TotalMilliseconds > 100)
				{
					// говорим флагу что мы начали рисовать линию
					_drawingMode = true;
					CnvDraw.Children.Remove(_tempLine);
					// мы рисуем первую линию
					if (_firstLine)
					{
						_prevX = e.GetPosition(CnvDraw).X;
						_prevY = _firstPoint.Y;
						_newX = _prevX;
						_newY = e.GetPosition(CnvDraw).Y;
					}
						// мы рисуем не первую линию
					else
					{
						// рисуем вертикальную линию
						if (_isVertical)
						{
							_newX = _prevX;
							_newY = e.GetPosition(CnvDraw).Y;
						}
							// рисуем горизонтальную линию
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
					// если левая кнопка не нажата, и мы были в режиме рисовашек
				else if (_drawingMode)
				{
					_drawingMode = false;
					// Йухххууу! Мы нарисовали первую линию епта!
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
					line.StrokeThickness = 5;
					CnvDraw.Children.Add(line);
					CnvDraw.Children.Remove(_tempLine);
					_linesList.Add(new LineInfo(_prevX, _prevY, line));
					//определение ребаных координат линии
					if (Math.Abs(_firstPoint.X) < 0.02)
					{
						MessageBox.Show("Задайти область определения!");
						RemoveLastLine();
					}
					else
					{
						if (TextXmin.Text.Equals("") || TextXmax.Text.Equals("") || TextYmin.Text.Equals("") || TextYmax.Text.Equals(""))
						{
							MessageBox.Show("Вы не ввели все необходимые данные!", "Внимание", MessageBoxButton.OK);
						}
						else
						{
							var ci = new CultureInfo("en-US") { NumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." } };
							try
							{
								var xmin = Convert.ToDouble(TextXmin.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								var xmax = Convert.ToDouble(TextXmax.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								var ymin = Convert.ToDouble(TextYmin.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								var ymax = Convert.ToDouble(TextYmax.Text.Replace(",", ".").Replace("ю", ".").Replace("б", "."), ci);
								X.Content = ((_prevX - _firstPoint.X) / (_rect.Width) * (xmin - xmax) + xmin).ToString("N2")
									+ " ; " + ((_newX - _firstPoint.X) / (_rect.Width) * (xmin - xmax) + xmin).ToString("N2");
								Y.Content = (Math.Abs((_prevY - _firstPoint.Y) / (_rect.Height) * (ymin - ymax) + ymin)).ToString("N2")
									+ " ; " + (Math.Abs((_newY - _firstPoint.Y) / (_rect.Height) * (ymin - ymax) + ymin).ToString("N2"));
								var fs = new FileStream("output.txt", FileMode.Append);
								var sw = new StreamWriter(fs);
								sw.WriteLine(X.Content + "\t" + Y.Content);
								sw.Close();
								fs.Close();
							}
							catch (FormatException)
							{
								MessageBox.Show("вы ввели некорректные данные!");
							}
						}
					}
					// запоминаем координаты
					_prevX = _newX;
					_prevY = _newY;

					// переключаем режим рисования на обратный
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
		private void ButLoad_Click(object sender, RoutedEventArgs e)
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
					MessageBox.Show("Некорректный тип файла!");
				}
			}
		}

		/// <summary>
		///     Задание области определения функции
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ButCoord_Click(object sender, RoutedEventArgs e)
		{
			CnvDraw.Children.Remove(_rect);
			while (_linesList.Count != 0)
			{
				RemoveLastLine();
			}
			_rect = new Rectangle {Stroke = Brushes.Red, StrokeThickness = 5, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top};
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
		private void MainWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_drawRectangle)
			{
				_firstPoint = e.GetPosition(CnvDraw);
				MoveDrawRectangle(_firstPoint, _firstPoint);
			}
		}

		/// <summary>
		///     Отпускание левой кнопки мыши на канвасе, содержащем изображение всего экрана
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
					rectangle.X = (int) (_firstPoint.X); // косяк вадим
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
		///     Растянуть/Переместить прямоугольник на указанные координаты
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
					Canvas.SetLeft(_rect, pointLeftTop.X); // косяк Настя
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
	}
}