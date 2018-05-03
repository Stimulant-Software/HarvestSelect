using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using SGApp.Models.EF;

namespace SGApp.Repository.Application {
	public class BinRepository: RepositoryBase<Bin> {
		public override IQueryable<Bin> EntityCollection {
			get
			{
				return DbContext.Bins.AsQueryable();
			}
		}

		public override Bin GetById(int id) {
			return DbContext.Bins.SingleOrDefault(x => x.BinID == id);
		}

		public override List<Bin> GetByPredicate(string predicate) {
			var iq = DbContext.Bins.AsQueryable();
			return predicate.Length > 0 ? iq.Where(predicate, null).ToList() : iq.ToList();
		}

		protected override Bin DeleteRecord(Bin entity) {
			throw new System.NotImplementedException();
		}

		protected override Bin InsertRecord(Bin entity) {
			DbContext.Bins.Add(entity);
			DbContext.SaveChanges();
			return entity;
		}

		internal List<Bin> GetBins() {
			return DbContext.Bins.OrderBy(x => x.BinName).ToList();
		}

        internal List<Bin> GetFarmBins(int FarmID)
        {
            return DbContext.Bins.Where(x => x.FarmID == FarmID).OrderBy(x => x.BinName).ToList();
        }

        internal Bin GetNewBinRecord() {
			var rec = DbContext.Bins.Create();
			DbContext.Bins.Add(rec);
			return rec;
		}

		internal BinLoad GetNewBinLoadRecord() {
			var rec = DbContext.BinLoads.Create();
			DbContext.BinLoads.Add(rec);
			return rec;
		}

		protected override Bin UpdateRecord(Bin entity) {
			DbContext.SaveChanges();
			return entity;
		}

		internal int GetLastBinLoadTicketNumber(int binID) {
			var count = DbContext.BinLoads.Count(x => x.BinID == binID);
			if (count == 0) {
				return 0;
			}
			return DbContext.BinLoads.Where(x => x.BinID == binID).Max(x => x.TicketNumber);
		}

		public int SaveChanges() {
			return DbContext.SaveChanges();
		}

		internal BinDisbursement GetNewBinDisbursementRecord() {
			var rec = DbContext.BinDisbursements.Create();
			DbContext.BinDisbursements.Add(rec);
			return rec;
		}

		internal void UpdateBinCurrentPounds(BinLoad binLoad = null, BinDisbursement binDisb = null) {
			var binId = 0;
			var currentTicket = 0;
			var lastDisbDate = new DateTime(1/1/1900);
			var lastLoaded = new DateTime(1 / 1 / 1900);
			if (binLoad == null) {
				//	update for bin disbursement
				binId = binDisb.BinID;
				lastDisbDate = binDisb.DisbursementDate;
			}
			else {
				//	update for bin load
				binId = binLoad.BinID;
				currentTicket = binLoad.TicketNumber;
				lastLoaded = binLoad.DateLoaded;
			}

			var totalDisbs = 0;
			var totalAdds = 0;
			if (DbContext.BinDisbursements.Any(x => x.BinID == binId)) {
				totalDisbs = DbContext.BinDisbursements.Where(x => x.BinID == binId).Sum(x => x.Pounds);
			}
			if (DbContext.BinLoads.Any(x => x.BinID == binId)) {
				totalAdds = DbContext.BinLoads.Where(x => x.BinID == binId).Sum(x => x.PoundsLoaded);
			}
			var bin = DbContext.Bins.Single(x => x.BinID == binId);
			bin.CurrentPounds = totalAdds - totalDisbs;
			bin.CurrentTicket = binLoad != null ? currentTicket : bin.CurrentTicket;
			bin.LastDisbursement = binDisb != null ? lastDisbDate : bin.LastDisbursement;
			bin.LastLoaded = binLoad != null ? lastLoaded : bin.LastLoaded;
			DbContext.SaveChanges();
		}

		internal int GetDisbursementType(string disbursementType) {
			return DbContext.DisbursementTypes.Single(x => x.DisbursementTypeName == disbursementType).DisbursementTypeID;
		}

		internal List<BinLoad> GetBinLoads(int binId, int skip) {
			return DbContext.BinLoads.Where(x => x.BinID == binId).OrderByDescending(x => x.DateLoaded).Skip(skip).Take(10).ToList();
		}

		public Bin GetBinData(int binId) {
			return DbContext.Bins.Single(x => x.BinID == binId);
		}
	}
}