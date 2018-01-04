using System.Collections.Generic;
using WaFFLs.Generation.Models;

namespace WaFFLs.Generation
{
    public interface ISeasonRecordProvider
    {
        List<SeasonRecord> GetData();
    }

    public interface IGameRecordProvider
    {
        List<GameRecord> GetData();
    }

    public interface IIndividualGameRecordProvider
    {
        List<IndividualGameRecord> GetData();
    }

    public interface ICareerRecordProvider
    {
        List<CareerRecord> GetData();
    }

    public interface IStreakRecordProvider
    {
        List<StreakRecord> GetData();

    }
}
