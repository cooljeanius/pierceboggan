#Introduction
MonoKlout is a library for Version 2 of the Klout API. It is built using the Mono framework and JSON.NET, and is especially aimed at use in mobile applications, specifically using MonoTouch or Mono for Android. 

#Features
*Support for all Version 2 calls
	*Identity
	*Score
		*Daily, Weekly, and Monthly changes
	*Influencers
	*Influencees
	*User Topics
*Works with the Mono framework and specifically at MonoTouch and Mono for Android (see [Xamarin][http://xamarin.com/])

#Getting Started
To get started, just instantiate the KloutAPI class.
    var klout = new KloutAPI("apikey", "twitterusername");
You can now access methods via the klout object:
    KloutIdentityResponse identity = klout.GetKloutIdentity();
    KloutScoreResponse score = klout.GetKloutScore();
    KloutInfluenceResponse influence = klout.GetInfluence();
    List<KloutUserTopicsResponse> userTopics = klout.GetUserTopics();

    // Klout Id
    string id = identity.id;

    // Klout Score
    string scoreNumber = score.score;
    string dailyChange = score.scoreDelta.dayChange;
    string weeklyChange = score.scoreDelta.weekChange;
    string monthlyChange = score.scoreDelta.monthChange;

    // Klout Influence
    var influenees = influence.myInfluencees;
    var influencers = influence.myInfluencers;

    foreach (KloutInfluenceEntityResponse influencee in influenees)
    {
	// Information about influencee
	Console.WriteLine(influencee.entity.payload.kloutId);
	Console.WriteLine(influencee.entity.payload.nick);
	Console.WriteLine(influencee.entity.payload.score);
    }

    foreach (KloutInfluenceEntityResponse influencer in influencers)
    {
	// Information about influencee
	Console.WriteLine(influencer.entity.payload.kloutId);
	Console.WriteLine(influencer.entity.payload.nick);
	Console.WriteLine(influencer.entity.payload.score);
    }

    // Klout User Topics
    foreach (KloutUserTopicsResponse topic in userTopics)
    {   
	// Information about the topic
	Console.WriteLine(topic.displayName);
	Console.WriteLine(topic.name);
	Console.WriteLine(topic.slug);
	Console.WriteLine(topic.imageUrl);
    }

#Roadmap
*Async
*Identity calls based off Google Plus identity

#Author
I (Pierce Boggan) wrote MonoKlout one weekend as a little side project. I'm currently a sophomore studying Software Engineering at Auburn University. You can visit my blog at pierceboggan.com.