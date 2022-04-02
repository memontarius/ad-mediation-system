namespace Virterix.AdMediation {
    public class DummyFetchStrategy : IFetchStrategy {
        public int TierIndex => 0;

        public int UnitIndex => 0;

        public void Init(AdUnit[][] tiers, int totalunits, int[] tierMaxPassages) {
        }

        public void Reset(AdUnit[][] tiers, int tierIndex, int unitIndex) {
        }

        public AdUnit Fetch(AdUnit[][] tiers) {
            return null;
        }
    }
}
