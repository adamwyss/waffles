using System.Collections.Generic;
using WaFFLs.Generation.Models;

namespace WaFFLs.Generation
{
    public interface IProvider
    {
    }

    public interface ISeasonRecordProvider : IProvider
    {
        List<SeasonRecord> GetData();
    }

    public interface IGameRecordProvider : IProvider
    {
        List<GameRecord> GetData();
    }

    public interface IIndividualGameRecordProvider : IProvider
    {
        List<IndividualGameRecord> GetData();
    }

    public interface ICareerRecordProvider : IProvider
    {
        List<CareerRecord> GetData();
    }

    public interface IStreakRecordProvider : IProvider
    {
        List<StreakRecord> GetData();
    }
}
