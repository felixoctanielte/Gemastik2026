using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.Data;
using UnityEngine;

namespace PeduliTransit.Events
{
    public static class IncidentCatalog
    {
        public static List<IncidentDefinition> GetForMode(TransportMode mode)
        {
            string vehicle = mode switch
            {
                TransportMode.Krl => "KRL",
                TransportMode.Bus => "bus TransJakarta",
                _ => "angkutan umum"
            };

            return new List<IncidentDefinition>
            {
                new IncidentDefinition
                {
                    id = $"{mode}_loud",
                    title = "Suara Keras",
                    category = EventCategory.Report,
                    npcRole = NpcRole.LoudTalking,
                    introStory =
                        $"Di dalam {vehicle}, ada penumpang yang berteriak dan berbicara sangat keras hingga mengganggu kenyamanan orang lain.",
                    decisionPrompt = "Apakah anda ingin melaporkan terkait hal tersebut?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Bagus! Melapor membantu petugas menegur perilaku yang mengganggu. Kendaraan umum adalah ruang bersama.",
                    storyAfterNo =
                        "Dengan tidak melapor, gangguan terus terjadi. Edukasi: suarakan kepedulian dengan cara yang aman.",
                    storyAfterTimeout =
                        "Waktu habis. Ingat: jika ada yang berteriak mengganggu, kamu bisa melapor ke petugas atau lewat kanal resmi."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_priority",
                    title = "Kursi Prioritas",
                    category = EventCategory.Report,
                    npcRole = NpcRole.PrioritySeatAbuse,
                    introStory =
                        $"Seseorang terduduk di kursi prioritas padahal terlihat mampu, sementara lansia berdiri di dekatnya di {vehicle}.",
                    decisionPrompt = "Apakah anda ingin melaporkan terkait hal tersebut?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Tepat. Kursi prioritas ditujukan bagi lansia, ibu hamil, disabilitas, dan penumpang berkebutuhan khusus.",
                    storyAfterNo =
                        "Membiarkan pelanggaran kursi prioritas mengurangi rasa aman kelompok rentan. Lapor atau ingatkan dengan sopan.",
                    storyAfterTimeout =
                        "Edukasi: kursi prioritas bukan sembarang duduk. Jika ada pelanggaran, laporkan atau tawarkan bantuan."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_phone",
                    title = "Volume HP",
                    category = EventCategory.Report,
                    npcRole = NpcRole.PhoneVolume,
                    introStory =
                        $"Musik dari speaker HP terdengar keras di {vehicle}. Beberapa penumpang terlihat terganggu.",
                    decisionPrompt = "Apakah anda ingin melaporkan terkait hal tersebut?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Bagus. Gunakan earphone di ruang publik. Melapor membantu menjaga kenyamanan bersama.",
                    storyAfterNo =
                        "Volume HP tanpa earphone mengganggu fokus dan ketenangan orang lain. Lebih baik dilapor atau diingatkan.",
                    storyAfterTimeout =
                        "Edukasi: di kendaraan umum, setel HP ke mode senyap atau pakai earphone."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_harass",
                    title = "Perilaku Tidak Pantas",
                    category = EventCategory.Report,
                    npcRole = NpcRole.HarassmentHint,
                    introStory =
                        $"Kamu melihat perilaku tidak pantas yang membuat penumpang lain terlihat tidak nyaman di {vehicle}.",
                    decisionPrompt = "Apakah anda ingin melaporkan terkait hal tersebut?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Benar. Keamanan penumpang prioritas. Laporkan ke petugas dengan tenang dan utamakan keselamatan.",
                    storyAfterNo =
                        "Mengabaikan perilaku tidak pantas berisiko memperparah situasi. Edukasi: lapor, jaga jarak aman, minta bantuan.",
                    storyAfterTimeout =
                        "Edukasi: jika melihat pelecehan atau perilaku tidak pantas, lapor segera ke petugas. Jangan menjadi pelaku atau penonton pasif."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_pregnant",
                    title = "Ibu Hamil",
                    category = EventCategory.Initiative,
                    npcRole = NpcRole.Pregnant,
                    introStory =
                        $"Seorang ibu hamil berdiri di dekatmu. Ada kursi kosong di dekat tempatmu di {vehicle}.",
                    decisionPrompt = "Apakah anda ingin memberikan tempat duduk?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Inisiatif bagus! Memberi tempat duduk adalah bentuk kepedulian sederhana yang sangat berarti.",
                    storyAfterNo =
                        "Ibu hamil membutuhkan kenyamanan dan keamanan lebih. Tawarkan kursi bila kamu mampu.",
                    storyAfterTimeout =
                        "Edukasi: utamakan ibu hamil, lansia, disabilitas, dan yang menggenggam anak kecil untuk duduk."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_child",
                    title = "Menggendong Anak",
                    category = EventCategory.Initiative,
                    npcRole = NpcRole.CarryingChild,
                    introStory =
                        $"Penumpang yang sedang menggendong anak kecil terlihat kewalahan berdiri di {vehicle}.",
                    decisionPrompt = "Apakah anda ingin memberikan tempat duduk?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Kerja bagus. Membantu pengasuh anak kecil mengurangi risiko kelelahan dan kecelakaan kecil.",
                    storyAfterNo =
                        "Menggendong anak sambil berdiri berat. Inisiatif memberi kursi menjaga keselamatan bersama.",
                    storyAfterTimeout =
                        "Edukasi: jika melihat orang menggendong anak, tawarkan tempat dudukmu."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_disability",
                    title = "Penumpang Disabilitas",
                    category = EventCategory.Initiative,
                    npcRole = NpcRole.Disability,
                    introStory =
                        $"Penumpang dengan disabilitas mencari tempat yang lebih aman untuk duduk di {vehicle}.",
                    decisionPrompt = "Apakah anda ingin memberikan tempat duduk?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Excellent. Aksesibilitas dimulai dari sikap peduli sesama penumpang.",
                    storyAfterNo =
                        "Ruang publik harus inklusif. Memberi kursi adalah langkah konkret mendukung aksesibilitas.",
                    storyAfterTimeout =
                        "Edukasi: bantu penumpang disabilitas dengan ramah—tawarkan kursi atau arahkan ke area prioritas."
                },
                new IncidentDefinition
                {
                    id = $"{mode}_elderly",
                    title = "Lansia",
                    category = EventCategory.Initiative,
                    npcRole = NpcRole.Elderly,
                    introStory =
                        $"Seorang lansia berdiri tidak jauh darimu sementara kamu duduk di {vehicle}.",
                    decisionPrompt = "Apakah anda ingin memberikan tempat duduk?",
                    timeLimit = 10f,
                    storyAfterYes =
                        "Sopan dan tepat. Menghormati lansia adalah nilai utama di ruang publik.",
                    storyAfterNo =
                        "Lansia berisiko lelah atau goyah saat kendaraan bergerak. Beri tempat duduk bila bisa.",
                    storyAfterTimeout =
                        "Edukasi: lihat sekeliling—jika ada lansia berdiri, segera tawarkan kursi."
                }
            };
        }
    }
}
