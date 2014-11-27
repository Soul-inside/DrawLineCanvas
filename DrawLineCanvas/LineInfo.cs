using System.Windows.Shapes;

namespace DrawLineCanvas
{
	/// <summary>
	///     ���������� � �����
	/// </summary>
	internal class LineInfo
	{
		/// <summary>
		///     X ���������� ������ �����
		/// </summary>
		public readonly double StartX;

		/// <summary>
		///     Y ���������� ������ �����
		/// </summary>
		public readonly double StartY;

		/// <summary>
		///     ������ �� �����
		/// </summary>
		public readonly Line LineReference;

		/// <summary>
		///     ����������� ��� �������� ����� ������ � ����
		/// </summary>
		/// <param name="startX">X ���������� ������ �����</param>
		/// <param name="startY">Y ���������� ������ �����</param>
		/// <param name="lineReference">������ �� �����</param>
		public LineInfo(double startX, double startY, Line lineReference)
		{
			StartX = startX;
			StartY = startY;
			LineReference = lineReference;
		}
	}
}