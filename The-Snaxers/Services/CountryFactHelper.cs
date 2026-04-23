public static class CountryFactHelper
{
    // Vi returnerar ett objekt med både fakta och bildkod
    public static (string Fact, string FlagCode) GetCountryDetails(string country)
    {
        return country?.ToLower() switch
        {
            "belgien" or "belgium" or "be" => ("Belgisk choklad är känd för sina eleganta praliner och höga kvalitet. Landet har en lång tradition av hantverk där fyllningar som nougat, karamell och likör är vanliga.", "be"),
            "schweiz" or "switzerland" or "ch" => ("Schweizisk choklad är berömd för sin krämighet och lena smak. Här utvecklades mjölkchokladen, och precisionen i tillverkningen är en stor del av ryktet.", "ch"),
            "ecuador" or "ecuador" or "ec" => ("Ecuador producerar fin kakao, särskilt sorten Arriba Nacional, en av världens mest exklusiva kakaosorter. Chokladen har ofta fruktiga och blommiga toner.", "ec"),
            "madagaskar" or "madagascar" or "mg" => ("Madagaskar är känt för sin kakaos tydliga syrlighet och bäriga smak. Chokladen är ofta livlig och komplex.", "mg"),
            "ghana" or "ghana" or "gh" => ("Ghana är en av världens största kakaoproducenter. Kakaon ger en fyllig, klassisk chokladsmak som används globalt.", "gh"),
            "sverige" or "sweden" or "se" => ("Sverige har en växande chokladscen med fokus på hantverk och hållbarhet. Små producenter experimenterar gärna med nordiska smaker.Svensk hantverkschoklad vinner ofta priser för sin renhet och kvalitet.", "se"),
            "venezuela" or "venezuela" or "ve" => ("Venezuela är känt för sin fina kakao, särskilt sorten Criollo, som anses vara en av de bästa i världen. Chokladen har ofta komplexa smaker med inslag av frukt och nötter.", "ve"),
            "peru" or "peru" or "pe" => ("Peru producerar högkvalitativ kakao, särskilt i regioner som Amazonas. Chokladen har ofta fruktiga och blommiga toner, och landet satsar på hållbar produktion.", "pe"),
            "tanzania" or "tanzania" or "tz" => ("Tanzania är en växande producent av fin kakao, särskilt i regioner som Mbeya. Tanzanisk kakao är mindre känd men uppskattad för sin rena, ibland citrusaktiga profil. Perfekt för exklusiva småskaliga chokladkakor.", "tz"),
            "vietnam" or "vietnam" or "vn" => ("Vietnam har snabbt blivit en “hidden gem” inom craft chocolate. Smakerna är ofta intensiva och kan dra åt tropisk frukt och kryddor.", "vn"),
            "filippinerna" or "philippines" or "ph" => ("Filippinerna producerar sällsynt kakao med djup och komplex smak. Lokala producenter gör ofta små batcher med tydlig terroir.", "ph"),
            "dominikanska republiken" or "dominican republic" or "do" => ("Känt för ekologisk kakao och hög kvalitet. Chokladen är ofta balanserad med toner av nötter, frukt och ibland lite syra.", "do"),
            "italien" or "italy" or "it" => ("Italien har en stark och elegant chokladtradition, särskilt i Piemonte. Här skapades gianduja – en blandning av choklad och hasselnöt som ger en mjuk, nötig smak. Landet är också känt för lyxiga praliner och hantverkschoklad från märken som Venchi och Amedei. Fokus ligger ofta på balans, textur och råvarukvalitet.", "it"),
            "japan" or "japan" or "jp" => ("Japan har ingen gammal kakaoproduktion men en extremt sofistikerad chokladkultur. Precision, estetik och säsong spelar stor roll. Nama chocolate (silkeslen, färsk choklad) är särskilt populär, och smaker som matcha och yuzu är vanliga.", "jp"),
            "finland" or "finland" or "fi" => ("Finland har en växande chokladscen med fokus på hantverk och hållbarhet. Små producenter experimenterar gärna med nordiska smaker som lingon, hjortron och enbär.", "fi"),
            "spanien" or "spain" or "es" => ("Spanien har en historisk koppling till choklad – det var hit kakao först kom till Europa från Amerika. Traditionellt dricks den som tjock choklad caliente, ofta tillsammans med churros. Idag finns även en växande craft-scen där producenter fokuserar på högkvalitativa bönor och rena smaker, ofta med influenser från Latinamerika.", "es"),
            "brasilien" or "brazil" or "br" => ("Brasilien är en av världens större kakaoproducenter men har också börjat satsa mer på exklusiv choklad. Särskilt regionen Bahia är känd för fin kakao med mjuka, fruktiga toner. På senare år har en ny generation bean-to-bar-tillverkare vuxit fram, med fokus på hållbarhet, ursprung och unik smakprofil.", "br"),
            _ => ("Ett spännande land med unika kakaotraditioner!", "un") // "un" för unknown
        };
    }
}