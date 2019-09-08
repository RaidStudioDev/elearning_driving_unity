using System.Collections.Generic;

public class DebugHandler
{
    public static bool isEnabled = false;

    public static void ParseUrlParameters(Dictionary<string, string> parameters)
    {
        // check if we have any special commands
        // debug mode is enabled via ?d=1
        // required: ?cid=1
        // &mode=winter || passenger || trucks &screen=game || tire
        if (parameters.ContainsKey("cid")) // challenge id
        {
            isEnabled = true;

            // set default user
            // check if we have a user already
            if (!parameters.ContainsKey("fullname"))
            {
                parameters.Add("fullname", "Tester");
                parameters.Add("email", "tester@sweetrush.com");
                parameters.Add("profile_field_region", "SweetRush");
                parameters.Add("profile_field_stateprovince", "SR");
            }

            // get challenge index, base 0
            PersistentModel.Instance.ChallengeIndex = int.Parse(parameters["cid"]) - 1;

            // get screen
            string screenName = parameters.ContainsKey("screen") ? parameters["screen"] : "game";
            switch (screenName)
            {
                case "tire":
                    PersistentModel.Instance.InitialScreen = UIManager.Screen.QUIZ_SCREEN;
                    break;
                case "game":
                    PersistentModel.Instance.InitialScreen = UIManager.Screen.GAME;
                    break;
            }

            // get mode
            string modeName = parameters.ContainsKey("mode") ? parameters["mode"] : "winter";
            switch (modeName)
            {
                case "winter":
                    PersistentModel.Instance.Mode = PersistentModel.ModeEnum.WINTER;
                    break;
                case "trucks":
                    PersistentModel.Instance.Mode = PersistentModel.ModeEnum.LIGHTTRUCK;
                    break;
                case "passenger":
                    PersistentModel.Instance.Mode = PersistentModel.ModeEnum.PASSENGER;
                    break;
            }

            // get challenge UID
            if (parameters.ContainsKey("uid"))
            {
                PersistentModel.Instance.ChallengeUID = parameters["uid"];
            }

            // disable randomize tracks
            PersistentModel.Instance.RandomizeTracks = false;
            PersistentModel.Instance.RandomizeTireOptions = false;

            // check for randomize tire options
            if (parameters.ContainsKey("rndOpts"))
            {
                int randOpts = int.Parse(parameters["rndOpts"]);
                PersistentModel.Instance.RandomizeTireOptions = (randOpts > 0) ? true : false;
            }
        }
    }
	
}
