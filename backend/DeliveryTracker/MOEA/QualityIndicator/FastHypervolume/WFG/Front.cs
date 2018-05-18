using System;
using System.Collections.Generic;
using System.IO;
using DeliveryTracker.MOEA.Core;

namespace DeliveryTracker.MOEA.QualityIndicator.FastHypervolume.WFG
{

	public class Front
	{
		private int dimension;
		private bool maximizing;
		public int NPoints
		{
			get;
			set;
		}

		public int NumberOfObjectives
		{
			get { return this.dimension; }
		}

		public int NumberOfPoints
		{
			get;
			private set;
		}

		public Point[] Points
		{
			get;
			private set;
		}

		private IComparer<Point> pointComparator;

		public Front()
		{
			this.maximizing = true;
			this.pointComparator = new PointComparator(this.maximizing);

		}

		public Front(int numberOfPoints, int dimension, SolutionSet solutionSet)
		{
			this.maximizing = true;
			this.pointComparator = new PointComparator(this.maximizing);
			this.NumberOfPoints = numberOfPoints;
			this.dimension = dimension;
			this.NPoints = numberOfPoints;

			this.Points = new Point[numberOfPoints];
			for (int i = 0; i < numberOfPoints; i++)
			{
				double[] p = new double[dimension];
				for (int j = 0; j < dimension; j++)
				{
					p[j] = solutionSet.Get(i).Objective[j];
				}
				this.Points[i] = new Point(p);
			}
		}

		public Front(int numberOfPoints, int dimension)
		{
			this.maximizing = true;
			this.pointComparator = new PointComparator(this.maximizing);
			this.NumberOfPoints = numberOfPoints;
			this.dimension = dimension;
			this.NPoints = numberOfPoints;
			this.Points = new Point[numberOfPoints];
			for (int i = 0; i < numberOfPoints; i++)
			{
				double[] p = new double[dimension];
				for (int j = 0; j < dimension; j++)
				{
					p[j] = 0.0;
				}
				this.Points[i] = new Point(p);
			}
		}

		public Front(int numberOfPoints, int dimension, List<double[]> listOfPoints)
		{
			this.maximizing = true;
			this.pointComparator = new PointComparator(this.maximizing);
			this.NumberOfPoints = numberOfPoints;
			this.dimension = dimension;
			this.Points = new Point[numberOfPoints];
			for (int i = 0; i < numberOfPoints; i++)
			{
				this.Points[i] = new Point(listOfPoints[i]);
			}
		}


		public void ReadFront(string fileName)
		{
			using (StreamReader reader = new StreamReader(fileName))
			{
				List<double[]> list = new List<double[]>();
				int numberOfObjectives = 0;
				string aux = reader.ReadLine();
				while (aux != null)
				{
					string[] st = aux.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
					int i = 0;
					numberOfObjectives = st.Length;

					double[] vector = new double[st.Length];
					foreach (string s in st)
					{
						double value = MOEA.Utils.Utils.ParseDoubleInvariant(s);
						vector[i] = value;
						i++;
					}
					list.Add(vector);
					aux = reader.ReadLine();
				}
				this.NumberOfPoints = list.Count;
				this.dimension = numberOfObjectives;
				this.Points = new Point[this.NumberOfPoints];
				this.NPoints = this.NumberOfPoints;
				for (int i = 0; i < this.NumberOfPoints; i++)
				{
					this.Points[i] = new Point(list[i]);
				}
			}
		}

		public void LoadFront(SolutionSet solutionSet, int notLoadingIndex)
		{

			if (notLoadingIndex >= 0 && notLoadingIndex < solutionSet.Size())
			{
				this.NumberOfPoints = solutionSet.Size() - 1;
			}
			else
			{
				this.NumberOfPoints = solutionSet.Size();
			}

			this.NPoints = this.NumberOfPoints;
			this.dimension = solutionSet.Get(0).NumberOfObjectives;

			this.Points = new Point[this.NumberOfPoints];

			int index = 0;
			for (int i = 0; i < solutionSet.Size(); i++)
			{
				if (i != notLoadingIndex)
				{
					double[] vector = new double[this.dimension];
					for (int j = 0; j < this.dimension; j++)
					{
						vector[j] = solutionSet.Get(i).Objective[j];
					}
					this.Points[index++] = new Point(vector);
				}
			}
		}

		public void PrintFront()
		{
			Console.WriteLine("Objectives:       " + this.dimension);
			Console.WriteLine("Number of points: " + this.NumberOfPoints);

			for (int i = 0, li = this.Points.Length; i < li; i++)
			{
				Console.WriteLine(this.Points[i]);
			}
		}

		public void SetToMazimize()
		{
			this.maximizing = true;
			this.pointComparator = new PointComparator(this.maximizing);
		}

		public void SetToMinimize()
		{
			this.maximizing = false;
			this.pointComparator = new PointComparator(this.maximizing);
		}

		public void Sort()
		{
			Array.Sort<Point>(this.Points, this.pointComparator);
		}

		public Point getReferencePoint()
		{
			Point referencePoint = new Point(this.dimension);

			double[] maxObjectives = new double[this.NumberOfPoints];
			for (int i = 0; i < this.NumberOfPoints; i++)
				maxObjectives[i] = 0;

			for (int i = 0; i < this.Points.Length; i++)
			{
				for (int j = 0; j < this.dimension; j++)
				{
					if (maxObjectives[j] < this.Points[i].Objectives[j])
					{
						maxObjectives[j] = this.Points[i].Objectives[j];
					}
				}
			}

			for (int i = 0; i < this.dimension; i++)
			{
				referencePoint.Objectives[i] = maxObjectives[i];
			}

			return referencePoint;
		}
	}
}
