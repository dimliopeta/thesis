using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChallengeLoader
{
    public static QueryResultChallengeContext[] LoadQueryResultChallenges()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("NPCChallenges/query_result_challenges");
        return JsonHelper.FromJson<QueryResultChallengeContext>(jsonFile.text);
    }

    public static TableMatchingContext[] LoadTableMatchingChallenges()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("NPCChallenges/table_matching_challenges");
        return JsonHelper.FromJson<TableMatchingContext>(jsonFile.text);
    }

    public static QueryColorGameContext[] LoadQueryColorChallenges()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("NPCChallenges/query_color_challenges");
        return JsonHelper.FromJson<QueryColorGameContext>(jsonFile.text);
    }
}
