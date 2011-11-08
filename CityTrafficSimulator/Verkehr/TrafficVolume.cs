﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

using CityTrafficSimulator.Vehicle;

namespace CityTrafficSimulator.Verkehr
	{
	/// <summary>
	/// This class encapsulates the traffic volume between two given BunchOfNode entities.
	/// </summary>
	[Serializable]
	public class TrafficVolume : ITickable
		{
		/// <summary>
		/// Random number generator
		/// </summary>
		private static Random rnd = new Random();


		/// <summary>
		/// Multiplikator für Fahrzeuge/Stunde
		/// </summary>
		public static decimal trafficDensityMultiplier = 1;

		/// <summary>
		/// Start nodes of the traffic volume
		/// </summary>
		[XmlIgnore]
		public BunchOfNodes startNodes;

		/// <summary>
		/// Destination nodes of the traffic volume
		/// </summary>
		[XmlIgnore]
		public BunchOfNodes destinationNodes;

		/// <summary>
		/// Car traffic volume in vehicles/hour
		/// </summary>
		private int m_trafficVolumeCars;
		/// <summary>
		/// Car traffic volume in vehicles/hour
		/// </summary>
		public int trafficVolumeCars
			{
			get { return m_trafficVolumeCars; }
			set { m_trafficVolumeCars = value; }
			}

		/// <summary>
		/// Truck traffic volume in vehicles/hour
		/// </summary>
		private int m_trafficVolumeTrucks;
		/// <summary>
		/// Truck traffic volume in vehicles/hour
		/// </summary>
		public int trafficVolumeTrucks
			{
			get { return m_trafficVolumeTrucks; }
			set { m_trafficVolumeTrucks = value; }
			}

		/// <summary>
		/// Bus traffic volume in vehicles/hour
		/// </summary>
		private int m_trafficVolumeBusses;
		/// <summary>
		/// Bus traffic volume in vehicles/hour
		/// </summary>
		public int trafficVolumeBusses
			{
			get { return m_trafficVolumeBusses; }
			set { m_trafficVolumeBusses = value; }
			}

		/// <summary>
		/// Tram traffic volume in vehicles/hour
		/// </summary>
		private int m_trafficVolumeTrams;
		/// <summary>
		/// Tram traffic volume in vehicles/hour
		/// </summary>
		public int trafficVolumeTrams
			{
			get { return m_trafficVolumeTrams; }
			set { m_trafficVolumeTrams = value; }
			}

		/// <summary>
		/// Queue of vehicles which have been created but could not be put on network because network was full
		/// </summary>
		private Queue<IVehicle> queuedVehicles = new Queue<IVehicle>();

		#region Konstruktoren

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="start">start nodes</param>
		/// <param name="destination">Destination nodes</param>
		public TrafficVolume(BunchOfNodes start, BunchOfNodes destination)
			{
			this.startNodes = start;
			this.destinationNodes = destination;
			this.m_trafficVolumeCars = 0;
			this.m_trafficVolumeTrucks = 0;
			this.m_trafficVolumeBusses = 0;
			this.m_trafficVolumeTrams = 0;
			}

		/// <summary>
		/// DO NOT USE: Empty Constructor - only needed for XML Serialization
		/// </summary>
		public TrafficVolume()
			{
			}

		/// <summary>
		/// Sets the traffic volume for each vehicle type
		/// </summary>
		/// <param name="cars">Car traffic volume in vehicles/hour</param>
		/// <param name="trucks">Truck traffic volume in vehicles/hour</param>
		/// <param name="busses">Bus traffic volume in vehicles/hour</param>
		/// <param name="trams">Tram traffic volume in vehicles/hour</param>
		public void SetTrafficVolume(int cars, int trucks, int busses, int trams)
			{
			this.m_trafficVolumeCars = cars;
			this.m_trafficVolumeTrucks = trucks;
			this.m_trafficVolumeBusses = busses;
			this.m_trafficVolumeTrams = trams;
			}

		#endregion


		#region ITickable Member

		private bool SpawnVehicle(IVehicle v)
			{
			LineNode start = startNodes.nodes[rnd.Next(startNodes.nodes.Count)];
			if (start.nextConnections.Count > 0)
				{
				int foo = rnd.Next(start.nextConnections.Count);
				NodeConnection nc = start.nextConnections[foo];

				v.state = new IVehicle.State(nc, 0);
				v.physics = new IVehicle.Physics(nc.targetVelocity, nc.targetVelocity, 0);
				if (start.nextConnections[foo].AddVehicle(v))
					{
					v.targetNodes = destinationNodes.nodes;
					return true;
					}
				}

			return false;
			}

		/// <summary>
		/// Notification that the world time has advanced by tickLength.
		/// </summary>
		/// <param name="tickLength">Amount the time has advanced</param>
		public void Tick(double tickLength)
			{
			if (tickLength > 0)
				{
				// enqueue cars
				int randomValue = trafficVolumeCars > 0 ? rnd.Next((int)Math.Ceiling(3600.0 / (tickLength * trafficVolumeCars))) : -1;
				if (randomValue == 0)
					{
					queuedVehicles.Enqueue(new Car(new IVehicle.Physics()));
					}

				// enqueue trucks
				randomValue = trafficVolumeTrucks > 0 ? rnd.Next((int)Math.Ceiling(3600.0 / (tickLength * trafficVolumeTrucks))) : -1;
				if (randomValue == 0)
					{
					queuedVehicles.Enqueue(new Truck(new IVehicle.Physics()));
					}

				// enqueue busses
				randomValue = trafficVolumeBusses > 0 ? rnd.Next((int)Math.Ceiling(3600.0 / (tickLength * trafficVolumeBusses))) : -1;
				if (randomValue == 0)
					{
					queuedVehicles.Enqueue(new Bus(new IVehicle.Physics()));
					}

				// enqueue trams
				randomValue = trafficVolumeTrams > 0 ? rnd.Next((int)Math.Ceiling(3600.0 / (tickLength * trafficVolumeTrams))) : -1;
				if (randomValue == 0)
					{
					queuedVehicles.Enqueue(new Tram(new IVehicle.Physics()));
					}
				}

			// spawn queued vehicles
			if (queuedVehicles.Count > 0)
				{
				if (SpawnVehicle(queuedVehicles.Peek()))
					{
					queuedVehicles.Dequeue();
					}
				}
			}


		/// <summary>
		/// Is called after the tick.
		/// </summary>
		public void Reset()
			{
			// Nothing to do here
			}

		#endregion

		#region Save/Load stuff

		/// <summary>
		/// Hash code of start point
		/// </summary>
		public int startHash = -1;
		/// <summary>
		/// Hash code of destination point
		/// </summary>
		public int destinationHash = -1;

		/// <summary>
		/// Prepares the object for XML serialization.
		/// </summary>
		public void PrepareForSave()
			{
			startHash = startNodes.hashcode;
			destinationHash = destinationNodes.hashcode;
			}

		/// <summary>
		/// Recovers the references after XML deserialization.
		/// </summary>
		/// <param name="saveVersion">Version of the read file</param>
		/// <param name="startList">List of all start BunchOfNodes</param>
		/// <param name="destinationList">List of all destination BunchOfNodes</param>
		public void RecoverFromLoad(int saveVersion, List<BunchOfNodes> startList, List<BunchOfNodes> destinationList)
			{
			foreach (BunchOfNodes bof in startList)
				{
				if (bof.hashcode == startHash)
					startNodes = bof;
				}
			foreach (BunchOfNodes bof in destinationList)
				{
				if (bof.hashcode == destinationHash)
					destinationNodes = bof;
				}
			}

		#endregion
		}
	}