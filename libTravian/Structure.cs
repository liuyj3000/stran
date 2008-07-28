﻿/*
 * The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * 
 * The Initial Developer of the Original Code is [MeteorRain <msg7086@gmail.com>].
 * Copyright (C) MeteorRain 2007, 2008. All Rights Reserved.
 * Contributor(s): [MeteorRain].
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Collections;
using System.Reflection;


namespace libTravian
{
	public class Data
	{
		public WebProxy Proxy { get; set; }
		public string Server { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Cookie { get; set; }
		public int Tribe { get; set; }
		public bool isRomans
		{
			get
			{
				return Tribe == 1;
			}
		}
		public int UserID { get; set; }
		public Dictionary<int, TVillage> Villages { get; set; }
		public List<TEvent> Events { get; set; }
		public int MarketSpeed { get; set; }
		public int ServerTimeOffset { get; set; }
		public int NewIGMCount
		{
			get
			{
				return IGMData.Count;
			}
		}
		public Dictionary<int, TIGM> IGMData { get; set; }
		//public Travian UpCall { get; set; }
		public Data()
		{
			Villages = new Dictionary<int, TVillage>();
			Events = new List<TEvent>();
			ActiveDid = -1;
		}
		public int ActiveDid { get; set; }
	}

	public class TIGM
	{
		string Sender { get; set; }
		string Subject { get; set; }
		string Text { get; set; }
	}

	public class TEvent
	{
		public IExpression<bool> Condition { get; set; }
		public IAction Action { get; set; }
		public TStatus Status { get; set; }
		public DateTime NextRun { get; set; }
	}

	public interface IExpression<T>
	{
	}

	public interface IAction
	{
	}

	public class TStatus
	{
		public string Status { get; set; }
		public string Details { get; set; }
	}

	public class TVillage
	{
		public int ID { get; set; }
		public Travian UpCall { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public TPoint Coord
		{
			get
			{
				return new TPoint(X, Y);
			}
			set
			{
				X = value.X;
				Y = value.Y;
			}
		}
		public int Z
		{
			get
			{
				return 801 * (400 - Y) + (X + 401);
			}
			set
			{
				X = value % 801 - 401;
				Y = 400 - value / 801;
			}
		}

		public TResAmount ResourceCapacity
		{
			get
			{
				int [] capacity = new int[this.Resource.Length];
				for (int i = 0; i < capacity.Length; i++)
				{
					capacity[i] = this.Resource[i].Capacity;
				}

				return new TResAmount(capacity);
			}
		}

		//[Obsolete("The role function hasn't been used now. Don't use it.", false)]
		public string Role { get; set; }
		public string Name { get; set; }
		public bool isCapital { get; set; }
		//private Dictionary<int, TBuilding> m_buildings;
		public SortedDictionary<int, TBuilding> Buildings { get; set; }
		public Dictionary<int, TRU> Upgrades { get; set; }
		public int BlacksmithLevel { get; set; }
		public int ArmouryLevel { get; set; }
		public DateTime RefreshTime { get; set; }
		public TResource[] Resource { get; set; }
		public TInBuilding[] InBuilding { get; set; }
		public List<TQueue> Queue { get; set; }
		public int isBuildingInitialized { get; set; }
		public int isUpgradeInitialized { get; set; }
		public int isDestroyInitialized { get; set; }
		public int isTroopInitialized { get; set; }
		public void InitializeBuilding()
		{
			isBuildingInitialized = 1;
			UpCall.FetchVillageBuilding(ID);
		}
		public void InitializeUpgrade()
		{
			isUpgradeInitialized = 1;
			UpCall.FetchVillageUpgrade(ID);
		}
		public void InitializeDestroy()
		{
			isDestroyInitialized = 1;
			UpCall.FetchVillageDestroy(ID);
		}
		public void InitializeTroop()
		{
			isTroopInitialized = 1;
			UpCall.FetchVillageTroop(ID);
		}
		public int TimeCost(TResAmount ResCost)
		{
			int time = 0;
			for (int i = 0; i < 4; i++)
				if (ResCost.Resources[i] > Resource[i].CurrAmount)
				{
					int costtime = -1;
					if (Resource[i].Produce > 0)
						costtime = (ResCost.Resources[i] - Resource[i].CurrAmount) * 3600 / Resource[i].Produce;
					if (costtime < 0)
						costtime = 32767;
					if (costtime > time)
						time = costtime;
				}
			return time;
		}
		public TInBuilding[] RB = new TInBuilding[5];
		public TMarket Market;
		public TVillage()
		{
			Resource = new TResource[4];
			InBuilding = new TInBuilding[7];
			Queue = new List<TQueue>();
			Upgrades = new Dictionary<int, TRU>();
			Troops = new List<TTroop>();
			Market = new TMarket();
			for (int i = 1; i <= 10; i++)
				Upgrades[i] = new TRU();
		}

		public override string ToString()
		{
			return TypeViewer.ToString(this);
		}

		/// <summary>
		/// Show the number of queued tasks and upper/lower limit tags
		/// </summary>
		/// <param name="db">User DB storing the queue/limit info</param>
		/// <returns>Status string</returns>
		public string GetStatus(LocalDB db)
		{
			string status = this.Queue.Count.ToString();
			if (this.Queue.Count == 0)
			{
				string key = "v" + this.ID.ToString() + "Queue";
				if (db.ContainsKey(key) && db[key].Length > 0)
				{
					string qString = db[key];
					status = qString.Split('|').Length.ToString() + "*";
				}
			}

			if (this.Market.LowerLimit != null)
			{
				status = status + "v";
			}

			if (this.Market.UpperLimit != null)
			{
				status = status + "^";
			}

			return status;
		}

		public string Snapshot()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Basic data:");
			sb.AppendLine(TypeViewer.Snapshot(this));
			sb.AppendLine(TypeViewer.Snapshot(this.Market));
			if (isBuildingInitialized == 2)
			{
				sb.AppendLine("Market:");
				foreach (TMInfo info in this.Market.MarketInfo)
				{
					sb.Append("\t");
					sb.AppendLine(info.ToString());
				}
				sb.AppendLine("Building:");
				foreach (var b in Buildings)
				{
					sb.Append("\t");
					sb.Append(b.Key);
					sb.Append(": ");
					sb.AppendLine(b.Value.ToString());
				}
				sb.AppendLine("Upgrades:");
				foreach (var b in Upgrades)
				{
					sb.Append("\t");
					sb.Append(b.Key);
					sb.Append(": ");
					sb.AppendLine(b.Value.ToString());
				}
				sb.AppendLine("Resource:");
				foreach (var b in Resource)
				{
					sb.Append("\t");
					sb.AppendLine(b.ToString());
				}
				sb.AppendLine("InBuilding:");
				for (var i = 0; i < InBuilding.Length; i++)
				{
					sb.Append("\t");
					sb.Append(i);
					sb.Append(": ");
					if (InBuilding[i] == null)
						sb.AppendLine("NULL");
					else
						sb.AppendLine(InBuilding[i].ToString());
				}
				sb.AppendLine("RecentBuilt:");
				for (var i = 0; i < RB.Length; i++)
				{
					sb.Append("\t");
					sb.Append(i);
					sb.Append(": ");
					if (RB[i] == null)
						sb.AppendLine("NULL");
					else
						sb.AppendLine(RB[i].ToString());
				}
				sb.AppendLine("Queue:");
				foreach (var b in Queue)
				{
					sb.Append("\t");
					sb.AppendLine(b.ToString());
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Save task queue to user DB
		/// </summary>
		/// <param name="User DB"></param>
		public void SaveQueue(LocalDB db)
		{
			string key = "v" + ID.ToString() + "Queue";
			db[key] = this.EncodeQueue();
		}

		/// <summary>
		/// Restore task queue from user DB
		/// </summary>
		/// <param name="User DB"></param>
		public void RestoreQueue(LocalDB db)
		{
			string key = "v" + ID.ToString() + "Queue";
			if (db.ContainsKey(key))
			{
				this.DecodeQueue(db[key]);
			}
		}

		/// <summary>
		/// Export task queue to a text file
		/// </summary>
		/// <param name="filename">Text file name</param>
		public void SaveQueue(string filename)
		{
			using (StreamWriter sw = new StreamWriter(filename))
			{
				sw.Write(this.EncodeQueue().Replace("|","\r\n").Replace("<_!!!_>", "|"));
			}
		}

		/// <summary>
		/// Import task queue from a text file
		/// </summary>
		/// <param name="filename">Text file name</param>
		public void RestoreQueue(string filename)
		{
			using (StreamReader sr = new StreamReader(filename))
			{
				string encodedQueue = sr.ReadToEnd();
				encodedQueue = encodedQueue.Replace("|", "<_!!!_>").Replace("\r\n", "|");
				this.DecodeQueue(encodedQueue);
			}
		}

		public void SaveResourceLimits(LocalDB db)
		{
			string key;
			if (this.Market.LowerLimit != null)
			{
				key = "v" + ID.ToString() + "LowerLimit";
				db[key] = this.Market.LowerLimit.ToString();
			}

			if (this.Market.UpperLimit != null)
			{
				key = "v" + ID.ToString() + "UpperLimit";
				db[key] = this.Market.UpperLimit.ToString();
			}
		}

		public void RestoreResourceLimits(LocalDB db)
		{
			string key = "v" + ID.ToString() + "LowerLimit";
			if (db.ContainsKey(key))
			{
				this.Market.LowerLimit = TResAmount.FromString(db[key]);
			}

			key = "v" + ID.ToString() + "UpperLimit";
			if (db.ContainsKey(key))
			{
				this.Market.UpperLimit = TResAmount.FromString(db[key]);
			}
		}

		public List<TTroop> Troops { get; set; }

		/// <summary>
		/// Convert current task queue into a string
		/// </summary>
		/// <returns>Encoded task queue</returns>
		private string EncodeQueue()
		{
			StringBuilder sb = new StringBuilder();
			foreach (TQueue task in Queue)
			{
				if (sb.Length != 0)
					sb.Append('|');
				if (task.NewOptions == null)
					sb.Append(task.ToString().Replace("|", "<_!!!_>"));
				else
					sb.Append(task.NewOptions.Export().Replace("|", "<_!!!_>"));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Restore task queue from a previously encode string
		/// </summary>
		/// <param name="data">Encoded task queue</param>
		private void DecodeQueue(string data)
		{
			foreach (string taskStr in data.Split('|'))
			{
				TQueue task = TQueue.FromString(taskStr.Replace("<_!!!_>", "|"));
				if (task.QueueType == TQueueType.Building && task.Bid > 0 && ! this.Buildings.ContainsKey(task.Bid))
				{
					this.Buildings[task.Bid] = new TBuilding() { Gid = task.Gid };
				}

				task.NextExec = DateTime.Now.AddSeconds(15);
				this.Queue.Add(task);
			}
		}
	}

	public class TResource
	{
		public int Capacity { private set; get; }
		public int Amount;// { private set; get; }
		public int Produce { private set; get; }
		private DateTime UpdateTime;
		public TResource(int Produce, int Amount, int Capacity)
		{
			this.Produce = Produce;
			this.Amount = Amount;
			this.Capacity = Capacity;
			UpdateTime = DateTime.Now;
		}
		public void Write(int Amount)
		{
			this.Amount = Math.Min(Amount, Capacity);
			UpdateTime = DateTime.Now;
		}
		public TimeSpan LeftTime
		{
			get
			{
				if (Produce < 0)
					return new TimeSpan(0, 0, CurrAmount * 3600 / -Produce);
				else if (Produce > 0)
					return new TimeSpan(0, 0, (Capacity - CurrAmount) * 3600 / Produce);
				else
					return new TimeSpan(1, 0, 0, 0);
			}
		}
		public int CurrAmount
		{
			get
			{
				return Math.Max(0, Math.Min(Amount + (int)(DateTime.Now.Subtract(UpdateTime).TotalHours * Produce), Capacity));
			}
		}
		public override string ToString()
		{
			return TypeViewer.ToString(this);
		}

	}

	/// <summary>
	/// Index as Bid.
	/// </summary>
	public class TBuilding
	{
		public int Gid { get; set; }
		public int Level { get; set; }
		public bool InBuilding { get; set; }
		public override string ToString()
		{
			return TypeViewer.ToString(this);
			//return string.Format("GID:{0}, Level:{1}, InBuilding:{2}", Gid, Level, InBuilding);
		}
	}

	/// <summary>
	/// Index as in-building type.
	/// </summary>
	public class TInBuilding
	{
		/// <summary>
		/// Bid for building, Aid for upgrade.
		/// </summary>
		public int ABid { get; set; }
		public int Gid { get; set; }
		public int Level { get; set; }
		public DateTime FinishTime { get; set; }
		public override string ToString()
		{
			return TypeViewer.ToString(this);
			//return string.Format("GID:{0}, BID:{1}, Level:{2}, FinishTime:{3}", Gid, ABid, Level, FinishTime.ToString());
		}
		public string CancelURL { get; set; }
		public bool Cancellable
		{
			get
			{
				return !string.IsNullOrEmpty(CancelURL);
			}
		}
	}

	public enum TQueueType
	{
		None,
		Building,
		Destroy,
		UAttack,
		UDefense,
		Research,
		Party,
		Transfer,
		NpcTrade,
		Raid
	}

	/// <summary>
	/// A queued task
	/// </summary>
	public class TQueue
	{
		/// <summary>
		/// Represents AI build strategy
		/// </summary>
		public static readonly int AIBID = -1024;

		/// <summary>
		/// Building slot ranges 1 - 50(?)
		/// </summary>
		public int Bid { get; set; }

		/// <summary>
		/// Building type
		/// </summary>
		public int Gid { get; set; }

		/// <summary>
		/// Target building level
		/// </summary>
		public int TargetLevel { get; set; }

		/// <summary>
		/// For display only
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// Need to distinguish out-skirt (0) and within-village (1) building tasks 
		/// </summary>
		public int Type
		{
			get
			{
				if (QueueType == TQueueType.Building)
					return Bid < 19 && Bid > 0 ? 0 : Bid != AIBID ? 1 : 0;
				else
					return (int)QueueType;
			}
		}

		/// <summary>
		/// Task type
		/// </summary>
		public TQueueType QueueType { get; set; }

		/// <summary>
		/// Encoded extra task options (used by transfer only)
		/// </summary>
		public string ExtraOptions { get; set; }

		/// <summary>
		/// New Options for taking the place of the old option string
		/// </summary>
		public IOption NewOptions { get; set; }

		/// <summary>
		/// When the queued task is ready to go (for display only)
		/// </summary>
		public DateTime NextExec { get; set; }

		/// <summary>
		/// When set to true, the corresponding task will be temporarily suspended
		/// </summary>
		public bool Paused { get; set; }

		/// <summary>
		/// Default constructor (will not create a valid task object though)
		/// </summary>
		public TQueue()
		{
			this.QueueType = TQueueType.None;
			this.Bid = 0;
			this.Gid = 0;
			this.Status = "";
			this.ExtraOptions = "";
			this.NextExec = DateTime.MinValue;
			this.Paused = false;
		}

		/// <summary>
		/// Decode a queued task from an encoded string
		/// </summary>
		/// <param name="data">Encoded string</param>
		/// <returns>Decoded task</returns>
		public static TQueue FromString(string data)
		{
			TQueue task = new TQueue();

			foreach (string attribute in data.Split(','))
			{
				string[] kvpair = attribute.Trim().Split(':');
				if (kvpair.Length != 2)
				{
					continue;
				}

				switch (kvpair[0])
				{
					case "Bid":
						task.Bid = Convert.ToInt32(kvpair[1]);
						break;
					case "Gid":
						task.Gid = Convert.ToInt32(kvpair[1]);
						break;
					case "TargetLevel":
						task.TargetLevel = Convert.ToInt32(kvpair[1]);
						break;
					case "Status":
						task.Status = kvpair[1];
						break;
					case "QueueType":
						task.QueueType = (TQueueType)Enum.Parse(typeof(TQueueType), kvpair[1]);
						break;
					case "ExtraOptions":
						task.ExtraOptions = kvpair[1];
						break;
					case "Paused":
						task.Paused = Boolean.Parse(kvpair[1]);
						break;
				}
			}

			return task;
		}

		/// <summary>
		/// Encode all properties in format "name:value,name:value,..."
		/// </summary>
		/// <returns>Encoded string</returns>
		public override string ToString()
		{
			return TypeViewer.ToString(this);
		}
	}

	public class TRU
	{
		public bool CanResearch { get; set; }
		public bool Researched { get; set; }
		public int AttackLevel { get; set; }
		public int DefenceLevel { get; set; }
		public bool InUpgrading { get; set; }
		public TRU()
		{
			AttackLevel = -1;
			DefenceLevel = -1;
		}
		public override string ToString()
		{
			return TypeViewer.ToString(this);
		}
	}

	public class TMarket
	{
		public int SingleCarry { get; set; }
		public int ActiveMerchant { get; set; }
		public int MaxMerchant { get; set; }
		public List<TMInfo> MarketInfo { get; set; }

		/// <summary>
		/// When transfer outward, don't let remaining resource below this 
		/// </summary>
		public TResAmount UpperLimit { get; set; }

		/// <summary>
		/// Stop receiving transporations when current resource amount is higher than this
		/// </summary>
		public TResAmount LowerLimit { get; set; }

		/// <summary>
		/// How long will the next market event (e.g., merchan returns) happen
		/// </summary>
		public int MinimumDelay
		{
			get
			{
				DateTime nextEventAt = DateTime.MaxValue;
				foreach (TMInfo transfer in this.MarketInfo)
				{
					if (transfer.FinishTime < nextEventAt)
					{
						nextEventAt = transfer.FinishTime;
					}
				}

				if (nextEventAt < DateTime.MaxValue)
				{
					return (int)Math.Round((nextEventAt - DateTime.Now).TotalSeconds);
				}
				else
				{
					return 0;
				}
			}
		}

		public void tick(ref TVillage CV, int MarketSpeed)
		{
			for(int i = MarketInfo.Count - 1; i >= 0; i--)
			{
				TMInfo x = MarketInfo[i];
				if(x.FinishTime > DateTime.Now)
					continue;
				if(x.MType == TMType.MyBack)
				{
					MarketInfo.Remove(x);
					if(SingleCarry == 0)
						continue;
					Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + ActiveMerchant.ToString());
					ActiveMerchant += Convert.ToInt32(Math.Ceiling((double)(x.CarryAmount.Resources[0] + x.CarryAmount.Resources[1] + x.CarryAmount.Resources[2] + x.CarryAmount.Resources[3]) / SingleCarry));
					Console.WriteLine(DateTime.Now.ToLongTimeString() + " " + ActiveMerchant.ToString());
				}
				else if(x.MType == TMType.MyOut)
				{
					x.MType = TMType.MyBack;
					var distance = CV.Coord * x.Coord;
					var time = distance * 3600 / MarketSpeed;
					try
					{
						x.FinishTime = x.FinishTime.AddSeconds(time);
					}
					catch(Exception ex)
					{
						throw new InvalidOperationException(
							string.Format("{0}\r\nMarketSpeed:{1}\r\nMyCoord:{2}\r\nTargetCoord:{3}",
							ex.Message, MarketSpeed, CV.Coord, x.Coord));
					}

				}
				else
				{
					for(int j = 0; j < 4; j++)
						CV.Resource[j].Write(CV.Resource[j].CurrAmount + x.CarryAmount.Resources[j]);
					MarketInfo.Remove(x);
				}
			}
		}
		public TMarket()
		{
			SingleCarry = 0;
			ActiveMerchant = 0;
			MaxMerchant = 0;
			MarketInfo = new List<TMInfo>();
		}
	}
	public enum TMType
	{
		MyOut,
		MyBack,
		OtherCome
	};

	/// <summary>
	/// Stores the info of an ongoing transportation
	/// </summary>
	public class TMInfo
	{
		public TMType MType { get; set; }
		public TResAmount CarryAmount { get; set; }
		public TPoint Coord { get; set; }
		public string VillageName { get; set; }
		public DateTime FinishTime { get; set; }

		public override string ToString()
		{
			return String.Format(
				"{0},{1},{2},{3},{4}",
				this.FinishTime,
				this.CarryAmount,
				this.VillageName,
				this.Coord,
				this.MType);
		}
	}

	public class TTroop
	{
		public int Tribe;
		public int[] Troops;
		public TTroopType TroopType;
	}
	public enum TTroopType
	{
		My
	}

	public static class TypeViewer
	{
		public static string ToString(object sender)
		{
			StringBuilder sb = new StringBuilder();
			Type t = sender.GetType();
			var p = t.GetProperties();//BindingFlags.Public);
			foreach (var x in p)
			{
				if (x.GetIndexParameters().Length == 0)
				{
					if (sb.Length != 0)
						sb.Append(", ");
					sb.Append(x.Name);
					sb.Append(":");
					sb.Append(x.GetValue(sender, null));
				}
			}
			return sb.ToString();
		}
		public static string Snapshot(object sender)
		{
			StringBuilder sb = new StringBuilder();
			Type t = sender.GetType();
			var p = t.GetProperties();//BindingFlags.Public);
			foreach (var x in p)
			{
				if (x.PropertyType == typeof(int) ||
					x.PropertyType == typeof(string) ||
					x.PropertyType == typeof(bool) ||
					x.PropertyType == typeof(DateTime)
					)
				{
					if (sb.Length != 0)
						sb.Append(Environment.NewLine);
					sb.Append(x.Name);
					sb.Append(":");
					sb.Append(x.GetValue(sender, null));
				}
			}
			return sb.ToString();
		}
	}
}