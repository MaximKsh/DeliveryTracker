using System;
using System.IO;
using DeliveryTracker.MOEA.Core;
using DeliveryTracker.MOEA.Encoding.SolutionType;
using DeliveryTracker.MOEA.Encoding.Variable;
using DeliveryTracker.MOEA.Utils;

namespace DeliveryTracker.MOEA.Problems.MTSP
{
	/// <summary>
	/// Class representing a multi-objective TSP (Traveling Salesman Problem) problem.
	/// This class is tested with two objectives and the KROA150 and KROB150 
	/// instances of TSPLIB
	/// </summary>
	public class MTSP : Problem
	{
		public int numberOfCities;
		public double[][] distanceMatrix;
		public double[][] costMatrix;

		/// <summary>
		/// Creates a new mTSP problem instance. It accepts data files from TSPLIB
		/// </summary>
		/// <param name="solutionType"></param>
		/// <param name="file_distances"></param>
		/// <param name="file_cost"></param>
		public MTSP(string solutionType, string file_distances, string file_cost)
		{
			this.NumberOfVariables = 1;
			this.NumberOfObjectives = 2;
			this.NumberOfConstraints = 0;
			this.ProblemName = "mTSP";

			this.Length = new int[this.NumberOfVariables];

			this.distanceMatrix = this.ReadProblem(file_distances);
			this.costMatrix = this.ReadProblem(file_cost);
			Console.WriteLine(this.numberOfCities);
			this.Length[0] = this.numberOfCities;
			if (solutionType == "Permutation")
			{
				this.SolutionType = new PermutationSolutionType(this);
			}
			else
			{
				Console.WriteLine("Error: solution type " + solutionType + " is invalid");
				Logger.Log.Error("Error: solution type " + solutionType + " is invalid");
				Environment.Exit(-1);
			}
		}

		/// <summary>
		/// Evaluates a solution
		/// </summary>
		/// <param name="solution">The solution to evaluate</param>
		public override void Evaluate(Solution solution)
		{
			double fitness1;
			double fitness2;

			fitness1 = 0.0;
			fitness2 = 0.0;

			for (int i = 0; i < (this.numberOfCities - 1); i++)
			{
				int x;
				int y;

				x = ((Permutation)solution.Variable[0]).Vector[i];
				y = ((Permutation)solution.Variable[0]).Vector[i + 1];

				fitness1 += this.distanceMatrix[x][y];
				fitness2 += this.costMatrix[x][y];
			}
			int firstCity;
			int lastCity;

			firstCity = ((Permutation)solution.Variable[0]).Vector[0];
			lastCity = ((Permutation)solution.Variable[0]).Vector[this.numberOfCities - 1];
			fitness1 += this.distanceMatrix[firstCity][lastCity];
			fitness2 += this.costMatrix[firstCity][lastCity];

			solution.Objective[0] = fitness1;
			solution.Objective[1] = fitness2;
		}

		public double[][] ReadProblem(string file)
		{
			double[][] matrix = null;

			using (StreamReader reader = new StreamReader(file))
			{
				string[] tokens = reader.ReadToEnd().Split(' ');
				try
				{
					int index;

					index = Array.IndexOf<string>(tokens, "DIMENSION");

					index += 2;

					this.numberOfCities = int.Parse(tokens[index]);

					matrix = new double[this.numberOfCities][];

					for (int i = 0; i < this.numberOfCities; i++)
					{
						matrix[i] = new double[this.numberOfCities];
					}

					index = Array.IndexOf<string>(tokens, "SECTION");

					// Read the data
					double[] c = new double[2 * this.numberOfCities];

					for (int i = 0; i < this.numberOfCities; i++)
					{
						index++;
						int j = int.Parse(tokens[index]);

						index++;
						c[2 * (j - 1)] = Utils.Utils.ParseDoubleInvariant(tokens[index]);

						index++;
						c[2 * (j - 1) + 1] = Utils.Utils.ParseDoubleInvariant(tokens[index]);
					}

					double dist;
					for (int k = 0; k < this.numberOfCities; k++)
					{
						matrix[k][k] = 0;
						for (int j = k + 1; j < this.numberOfCities; j++)
						{
							dist = Math.Sqrt(Math.Pow((c[k * 2] - c[j * 2]), 2.0)
									+ Math.Pow((c[k * 2 + 1] - c[j * 2 + 1]), 2));
							dist = (int)(dist + .5);
							matrix[k][j] = dist;
							matrix[j][k] = dist;
						}
					}
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("TSP.ReadProblem(): error when reading data file " + e);
					Environment.Exit(-1);
				}
			}
			return matrix;
		}
	}
}
