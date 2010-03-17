﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

namespace libTravian
{
    public class BalancerQueue : IQueue
    {

        #region IQueue 成员

        public Travian UpCall { get; set; }

        [Json]
        public int VillageID { get; set; }

        public bool MarkDeleted { get; set; }

        [Json]
        public bool Paused { get; set; }

        public string Title
        {
            get { return "AutoBalancer"; }
        }

        public string Status
        {
            get
            {
                switch (type)
                {
                    case villagetype.full:
                        return "爆仓";
                    case villagetype.giver:
                        return "空闲";
                    case villagetype.marketnotavailable:
                        return "无空闲商人";
                    case villagetype.needer:
                        return "需求资源";
                    default:
                        return "等待处理";
                }
            }
        }

        public int CountDown
        {
            get
            {
                if (--delay < 0)
                {
                    delay = 5;
                    return 0;
                }
                else
                {
                    return delay;
                }
            }
        }
        int delay = 4;

        public void Action()
        {
            switch (state)
            {
                case states.notinitlized:
                    UpdateType();
                    break;
                case states.initlized:
                    execute();
                    break;
                //case states.executing:
                //    break;
                //case states.done:
                //    break;
                default:
                    UpdateType();
                    break;
            }
        }

        public int QueueGUID
        {
            get
            {
                return 5;
            }
        }

        #endregion

        #region fields

        private int NextExec;
        private int retrycount;

        private int BalancerGroup;//资源平衡组

        private int ignoreMarket { get; set; }//是否无视市场运输
        private int ignoreTime { get; set; }//无视市场运输的时间，即大于这个时间的不计算

        private TVillage village;//当前村庄
        private states state;//状态
        private villagetype type;//类型
        private TResAmount needRes = new TResAmount();//需要的资源

        private List<TSVillage> groupVillages;//本组的村庄
        private DateTime lastUpdateTime;
        #endregion


        #region enums

        public enum states
        {
            notinitlized = 0,
            initlized = 1,
            executing = 2,
            done = 4
        }

        public enum villagetype
        {
            needer = 1,
            giver = 2,
            marketnotavailable = 3,
            full = 4,
            unknown = 5,
        }

        #endregion

        #region methods
        public BalancerQueue()
        {
            groupVillages = new List<TSVillage>();
        }

        //private void debug(String message)
        //{
        //    if (message == null) return;
        //    UpCall.DebugLog(message, DebugLevel.E);
        //}

        private void UpdateType()
        {

            village = UpCall.TD.Villages[VillageID];
            if (village.Name.Contains("047"))
            {
                int debug = 3;
            }
            if (village == null) return;
            if (DateTime.Now.Subtract(lastUpdateTime).TotalSeconds > 10)
            {
                UpdateGroupVillages();
                lastUpdateTime = DateTime.Now;
            }
            /*
            if (village.isMarketInitialized() == false)
            {
                debug("market not initialized, wait 10 second");
            }
             */
            if (village.Queue == null)
            {
                type = UpdateMarketState();

            }
            else
            {
                ///检查建筑队列
                TResAmount source = CaculateBuildingAmount(ignoreMarket, ignoreTime);
                //检查party
                if (source.isZero())
                {
                    source = CaculatePartyResource(ignoreMarket, ignoreTime);
                }
                //检查研究
                if (source.isZero())
                {
                    source = CaculateResearchAmount(ignoreMarket, ignoreTime);
                }
                //检查造兵
                if (source.isZero())
                {
                    source = CaculateProduceTroop(ignoreMarket, ignoreTime);
                }
                needRes = source;
                if (source.isZero())
                {
                    //TODO 检查爆仓
                    type = UpdateMarketState();
                }
                else
                {

                    UpCall.DebugLog("Auto Balancer : " + VillageShort(village) + " need res " + source.ToString(), DebugLevel.E);
                    type = villagetype.needer;
                }
            }
            state = states.initlized;
        }

        protected bool notInBuilding(TInBuilding inbuilding)
        {
            if (inbuilding == null) return true;
            //提前60秒开始送资源
            //TODO 可以设置到配置文件中
            return inbuilding.end(-60);
        }

        //检测建筑序列所需的资源
        protected TResAmount CaculateBuildingAmount(int ignoreMarket, int ignorTime)
        {
            TResAmount resource = new TResAmount();
            if (UpCall.TD.isRomans)
            {
                //TODO罗马双造的处理
            }
            else
            {

            }
            //单造的处理
            if (notInBuilding(village.InBuilding[0]) && notInBuilding(village.InBuilding[1]))
            {
                foreach (var task in village.Queue)
                {
                    if (task.GetType().Name == "BuildingQueue")
                    {
                        BuildingQueue Q = task as BuildingQueue;
                        //UpCall.DebugLog(VillageShort(village) + " BuildingQ " + Q.Gid, DebugLevel.E);
                        TResAmount res = Buildings.Cost(village.Buildings[Q.Bid].Gid, village.Buildings[Q.Bid].Level + 1);
                        //建筑所需资源没有超过仓库上限
                        if ((larger(res, GetVillageCapacity(village))) == false)
                        {
                            resource += res;
                            break;
                        }
                    }
                    else if (task.GetType().Name == "AIQueue")
                    {
                        AIQueue Q = task as AIQueue;
                        if (Q.Gid != 0)
                        {
                            //UpCall.DebugLog(VillageShort(village) + " AIQueue " + Q.Gid, DebugLevel.E);
                            TResAmount res = Buildings.Cost(village.Buildings[Q.Bid].Gid, village.Buildings[Q.Bid].Level + 1);
                            //建筑所需资源没有超过仓库上限
                            if ((larger(res, GetVillageCapacity(village))) == false)
                            {
                                resource += res;
                                break;
                            }
                        }
                    }
                }
            }
            resource -= GetVillageRes(village, ignoreMarket, ignoreTime);
            resource.clearMinus();
            if (resource.isZero() == false)
            {
                UpCall.DebugLog(VillageShort(village) + " Building Need " + resource, DebugLevel.E);
            }
            return resource;
        }

        //检测研究序列所需的资源
        protected TResAmount CaculateResearchAmount(int ignoreMarket, int ignorTime)
        {
            TResAmount result = new TResAmount();
            //UPAttack
            if (notInBuilding(village.InBuilding[3]))
            {
                foreach (var task in village.Queue)
                {
                    if (task.GetType().Name == "ResearchQueue")
                    {
                        ResearchQueue Q = task as ResearchQueue;
                        if (Q.ResearchType == ResearchQueue.TResearchType.UpAttack)
                        {
                            TResAmount res = Buildings.UpCost[(UpCall.TD.Tribe - 1) * 10 + Q.Aid][village.Upgrades[Q.Aid].AttackLevel];
                            result = res - GetVillageRes(village, ignoreMarket, ignorTime);
                            break;
                        }
                    }
                }
            }

            //UPDefense
            if (result.isZero())
            {
                if (notInBuilding(village.InBuilding[4]))
                {
                    foreach (var task in village.Queue)
                    {
                        if (task.GetType().Name == "ResearchQueue")
                        {
                            ResearchQueue Q = task as ResearchQueue;
                            if (Q.ResearchType == ResearchQueue.TResearchType.UpDefence)
                            {
                                TResAmount res = Buildings.UpCost[(UpCall.TD.Tribe - 1) * 10 + Q.Aid][village.Upgrades[Q.Aid].DefenceLevel];
                                result = res - GetVillageRes(village, ignoreMarket, ignorTime);
                                break;
                            }
                        }
                    }
                }
                //Research
                if (result.isZero())
                {
                    if (notInBuilding(village.InBuilding[5]))
                    {
                        foreach (var task in village.Queue)
                        {
                            if (task.GetType().Name == "ResearchQueue")
                            {
                                ResearchQueue Q = task as ResearchQueue;
                                if (Q.ResearchType == ResearchQueue.TResearchType.Research)
                                {
                                    TResAmount res = Buildings.ResearchCost[(UpCall.TD.Tribe - 1) * 10 + Q.Aid];
                                    result = res - GetVillageRes(village, ignoreMarket, ignorTime);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            result.clearMinus();
            if (result.isZero() == false)
            {
                UpCall.DebugLog(VillageShort(village) + " Research Need " + result, DebugLevel.E);
            }
            return result;
        }

        //检测Party所需的资源
        protected TResAmount CaculatePartyResource(int ignoreMarket, int ignoreTime)
        {
            TResAmount result = new TResAmount();
            if (notInBuilding(village.InBuilding[6]))
            {
                foreach (var task in village.Queue)
                {
                    if (task.GetType().Name == "PartyQueue")
                    {
                        PartyQueue Q = task as PartyQueue;
                        //UpCall.DebugLog(VillageShort(village) + " PartyQ " + Q.PartyType, DebugLevel.E);
                        TResAmount res = Buildings.PartyCos[(int)Q.PartyType - 1];
                        if ((larger(res, GetVillageCapacity(village))) == false)
                        {
                            result = res - GetVillageRes(village, ignoreMarket, ignoreTime);
                            break;
                        }
                    }
                }
            }
            result.clearMinus();
            if (result.isZero() == false)
            {
                UpCall.DebugLog(VillageShort(village) + " Party Need " + result, DebugLevel.E);
            }
            return result;
        }

        protected TResAmount CaculateProduceTroop(int ignoreMarket, int ignoreTime)
        {
            TResAmount result = new TResAmount();
            foreach (var task in village.Queue)
            {
                if (task.GetType().Name == "ProduceTroopQueue")
                {
                    ProduceTroopQueue Q = task as ProduceTroopQueue;

                    var CV = UpCall.TD.Villages[VillageID];
                    int Aid = Q.Aid;
                    int Amount = Q.Amount;
                    int key = (UpCall.TD.Tribe - 1) * 10 + Aid;
                    int timecost;
                    TResAmount TroopRes;
                    if (Aid == 9 || Aid == 10)
                        TroopRes = Buildings.TroopCost[key] * Amount;
                    else
                        TroopRes = Buildings.TroopCost[key] * Amount * (Q.GRt ? 3 : 1);

                    result = TroopRes - GetVillageRes(village, ignoreMarket, ignoreTime);
                    break;
                }
            }
            result.clearMinus();
            if (result.isZero() == false)
            {
                UpCall.DebugLog(VillageShort(village) + " Produce Troop Need " + result, DebugLevel.E);
            }
            return result;
        }

        //返回目标村庄的资源上限
        protected TResAmount GetVillageCapacity(TVillage village)
        {
            return village.ResourceCapacity;
        }

        //返回村庄当前资源
        protected TResAmount GetVillageRes(TVillage village, int ignoreMarket, int ignorTime)
        {
            TResAmount res = new TResAmount(village.ResourceCurrAmount);
            //TODO ignoreMarket和ignoreTime的处理
            foreach (TMInfo transfer in village.Market.MarketInfo)
            {
                res += transfer.CarryAmount;
            }
            return res;
        }

        //检查市场
        private villagetype UpdateMarketState()
        {
            if (village.Market.ActiveMerchant <= 0)
            {
                return villagetype.marketnotavailable;
            }
            else
            {
                foreach (var res in village.Resource)
                {
                    if (res != null && res.isFull)
                    {
                        return villagetype.full;
                    }
                }
                return villagetype.giver;
            }
        }

        private void execute()
        {
            //PULL模式，自动寻找最近的资源点pull资源
            if (type == villagetype.needer)
            {
                TResAmount res = new TResAmount(needRes);
                foreach (var tsv in groupVillages)
                {
                    //tsv.queue.UpdateState();
                    TVillage fromVillage = UpCall.TD.Villages[tsv.VillageID];
                    if (tsv.queue.type == villagetype.giver
                        || tsv.queue.type == villagetype.full)
                    {

                        TResAmount r = GetVillageRes(fromVillage, ignoreMarket, ignoreTime);
                        int marketCarry = fromVillage.Market.ActiveMerchant * fromVillage.Market.SingleCarry;
                        //资源和商人都充足
                        if (res.TotalAmount < marketCarry && smaller(res, r))
                        {
                            DoTranfer(fromVillage, this.village, res);
                            res -= res;
                        }
                        else
                        {
                            int totalSend = 0;
                            int[] sendRes = new int[r.Resources.Length];
                            for (int i = 0; i < sendRes.Length; i++)
                            {
                                int thisTypeCount = needRes.Resources[i];
                                if (thisTypeCount > marketCarry)
                                {
                                    sendRes[i] = (marketCarry > r.Resources[i]) ? r.Resources[i] : marketCarry;
                                }
                                else
                                {
                                    sendRes[i] = (r.Resources[i] > thisTypeCount) ? thisTypeCount : r.Resources[i];
                                }
                                marketCarry -= sendRes[i];
                            }
                            TResAmount r2 = new TResAmount(sendRes);
                            DoTranfer(fromVillage, this.village, r2);
                            res -= r2;
                        }
                    }

                    if (res.isZero())
                    {
                        break;
                    }
                }
            }
            else if (type == villagetype.full)
            {
                //push模式，爆仓的村庄自动寻找资源最少的村子进行push
                if (village.Market.ActiveMerchant > 0)
                {
                    int outResType = -1;
                    for(int i=0;i<village.Resource.Length;i++){
                        if (village.Resource[i] != null && village.Resource[i].isFull)
                        {
                            outResType = i;
                        }
                    }

                    if(outResType != -1){
                        double minCap = 100.0;
                        int targetVillageID = -1;

                        foreach (var tsv in groupVillages)
                        {
                            TVillage tv = UpCall.TD.Villages[tsv.VillageID];
                            double rate = tv.Resource[outResType].CurrAmount * 100.0 / tv.Resource[outResType].Capacity;
                            if (rate < minCap)
                            {
                                minCap = rate;
                                targetVillageID = tsv.VillageID;
                            }
                        }

                        if (targetVillageID != -1)
                        {
                            //计算运送的资源
                            //TODO 没有加上ResourceLimit的计算
                            TVillage targetVillage = UpCall.TD.Villages[targetVillageID];
                            int maxReceiveRes = targetVillage.Resource[outResType].Capacity - targetVillage.Resource[outResType].CurrAmount;
                            maxReceiveRes = maxReceiveRes * 8 / 10;
                            int maxCarry = village.Market.ActiveMerchant * village.Market.SingleCarry;
                            int maxSend = village.Resource[outResType].CurrAmount;
                            maxSend = (maxCarry < maxSend) ? maxCarry : maxSend;
                            maxSend = (maxReceiveRes < maxSend) ? maxReceiveRes : maxSend;

                            TResAmount res = new TResAmount();
                            res.Resources[outResType] = maxSend;
                            UpCall.DebugLog("push res from " + VillageShort(village) + " => " + VillageShort(targetVillage) + " " + res, DebugLevel.E);
                            DoTranfer(village,targetVillage,res);
                        }
                    }

                }
                else
                {
                    UpCall.DebugLog(VillageShort(village) + " resource is full " + village.ResourceCurrAmount, DebugLevel.E);
                }
            }
            UpdateType();
        }

        private int GetMarketMan(int totalSend, int carry)
        {
            int c = totalSend / carry;
            int r = totalSend % carry;
            if (r == 0)
            {
                return c;
            }
            else
            {
                return c + 1;
            }
        }

        private void DoTranfer(TVillage from, TVillage to, TResAmount res)
        {
            UpCall.DebugLog("Balancer : " + VillageShort(from) + " => " + VillageShort(to) + " " + res.ToString(), DebugLevel.E);
            TransferQueue transfer = new TransferQueue()
            {
                UpCall = this.UpCall,
                VillageID = from.ID,
                TargetPos = to.Coord,
                ResourceAmount = res
            };
            transfer.Action();
        }

        //按照距离更新村庄列表
        public void UpdateGroupVillages()
        {
            if (groupVillages == null)
            {
                groupVillages = new List<TSVillage>();
            }
            else
            {
                groupVillages.Clear();
            }
            foreach (var vid in UpCall.TD.Villages.Keys)
            {
                TVillage village = UpCall.TD.Villages[vid];

                BalancerQueue Q = village.getBalancer();
                if (Q != null)
                {
                    TSVillage one = new TSVillage
                    {
                        VillageID = vid,
                        coord = village.Coord,
                        distance = village.Coord * this.village.Coord,
                        queue = Q
                    };
                    groupVillages.Add(one);
                }
            }
            groupVillages.Sort();
            foreach (var v in groupVillages)
            {
                TVillage TV = UpCall.TD.Villages[v.VillageID];
                //UpCall.DebugLog(TV.Name + v.distance,DebugLevel.E);
            }
            //debug("Auto Balancer : " + this.village.Name + " Update Group VillageList, size = " + groupVillages.Count);
        }

        #endregion


        public static bool larger(TResAmount r1, TResAmount r2)
        {
            for (int i = 0; i < r1.Resources.Length; i++)
            {
                if (r1.Resources[i] < r2.Resources[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool smaller(TResAmount r1, TResAmount r2)
        {
            for (int i = 0; i < r1.Resources.Length; i++)
            {
                if (r1.Resources[i] > r2.Resources[i])
                {
                    return false;
                }
            }
            return true;

        }

        //用于比较距离，记录坐标和距离
        public class TSVillage : IComparable<TSVillage>
        {
            public int VillageID;
            public TPoint coord;
            public double distance;
            public BalancerQueue queue;

            #region IComparable<TBVillage> 成员

            public int CompareTo(TSVillage other)
            {
                return (int)(distance - other.distance);
            }

            #endregion

            public String toString()
            {
                return queue.ToString() + VillageID;
            }
        }

        public override String ToString()
        {
            return village.Name + type;
        }

        public static string VillageShort(TVillage CV)
        {
            return string.Format("{0} ({1}|{2})", CV.Name, CV.Coord.X, CV.Coord.Y);
        }
    }
}