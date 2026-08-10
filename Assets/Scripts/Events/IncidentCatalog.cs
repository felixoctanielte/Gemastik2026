using System.Collections.Generic;
using PeduliTransit.Core;
using PeduliTransit.Data;

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

            string contact = mode switch
            {
                TransportMode.Krl => "Satpam KRL",
                TransportMode.Bus => "Petugas Bus",
                _ => "Kondektur Angkot"
            };

            string contactSub = "biasanya membalas cepat";

            return new List<IncidentDefinition>
            {
                MakeReport(
                    id: $"{mode}_loud",
                    title: "Suara Keras",
                    role: NpcRole.LoudTalking,
                    vehicle: vehicle,
                    contact: contact,
                    contactSub: contactSub,
                    intro: $"Di dalam {vehicle}, ada penumpang yang berteriak dan berbicara sangat keras hingga mengganggu kenyamanan orang lain.",
                    correctId: "loud",
                    correctLabel: "Lapor: suara mengganggu",
                    correctChat: $"Halo, di {vehicle} ada penumpang berteriak sangat keras dan mengganggu. Mohon dibantu tegur.",
                    escalate: false,
                    afterYes: "Bagus! Petugas datang menegur. Kendaraan umum adalah ruang bersama—laporan yang tepat membantu semua orang.",
                    afterWrong: "Laporan tidak sesuai kejadian. Edukasi: kirim pesan yang menggambarkan situasi dengan jujur ke petugas/penanggung jawab kendaraan. Salah lapor bisa membingungkan pihak berwajib (−10)."
                ),
                MakePriority(
                    mode, vehicle, contact, contactSub
                ),
                MakeReport(
                    id: $"{mode}_phone",
                    title: "Volume HP",
                    role: NpcRole.PhoneVolume,
                    vehicle: vehicle,
                    contact: contact,
                    contactSub: contactSub,
                    intro: $"Musik dari speaker HP terdengar keras di {vehicle}. Beberapa penumpang terlihat terganggu.",
                    correctId: "phone",
                    correctLabel: "Lapor: volume HP tanpa earphone",
                    correctChat: $"Mohon bantu, di {vehicle} ada yang nyalakan speaker HP terlalu kencang tanpa earphone.",
                    escalate: false,
                    afterYes: "Bagus. Petugas mengingatkan untuk memakai earphone. Melapor menjaga kenyamanan bersama.",
                    afterWrong: "Isi laporan tidak cocok dengan kejadian. Edukasi: sampaikan gejala yang kamu lihat (mis. suara HP keras), jangan pilih jenis laporan asal (−10)."
                ),
                MakeReport(
                    id: $"{mode}_harass",
                    title: "Pelecehan / Perilaku Tidak Pantas",
                    role: NpcRole.HarassmentHint,
                    vehicle: vehicle,
                    contact: contact,
                    contactSub: contactSub,
                    intro: $"Kamu melihat perilaku tidak pantas yang membuat penumpang lain terlihat tidak nyaman di {vehicle}.",
                    correctId: "harass",
                    correctLabel: "Lapor: dugaan pelecehan",
                    correctChat: $"Darurat: ada dugaan pelecehan / perilaku tidak pantas di {vehicle}. Mohon petugas segera ke lokasi.",
                    escalate: true,
                    afterYes: "Benar. Petugas menuju lokasi dan mengamankan situasi. Jika membesar, pelaku digiring keluar dan diserahkan ke penanggung jawab. Utamakan keselamatanmu.",
                    afterWrong: "Salah jenis laporan. Edukasi: untuk dugaan pelecehan, segera hubungi petugas resmi (satpam/petugas/kondektur) dengan pesan yang jelas. Jangan spekulasi berlebihan, tapi jangan diam (−10)."
                ),
                MakeReport(
                    id: $"{mode}_fight",
                    title: "Berantem",
                    role: NpcRole.Fighting,
                    vehicle: vehicle,
                    contact: contact,
                    contactSub: contactSub,
                    intro: $"Terjadi perkelahian / saling dorong di {vehicle}. Situasi mulai tegang dan penumpang lain menjauh.",
                    correctId: "fight",
                    correctLabel: "Lapor: perkelahian",
                    correctChat: $"Tolong segera! Ada orang berantem di {vehicle}. Mohon petugas datang damaikan / amankan.",
                    escalate: true,
                    afterYes: "Laporan tepat. Petugas melerai dan jika makin besar, pelaku digiring keluar kendaraan. Ruang publik bukan tempat menyelesaikan konflik dengan kekerasan.",
                    afterWrong: "Laporan tidak sesuai. Edukasi: saat ada perkelahian, lapor ke penanggung jawab kendaraan—jangan ikut terlibat. Pesan singkat, jelas, lokasi (−10)."
                ),
                MakeInitiative($"{mode}_pregnant", "Ibu Hamil", NpcRole.Pregnant, vehicle,
                    $"Seorang ibu hamil berdiri di dekatmu. Kursi prioritas seharusnya untuk beliau, tapi ada yang tidak berhak mendudukinya—atau ada kursi yang bisa kamu bantu sediakan di {vehicle}.",
                    "Apakah anda ingin membantu ibu hamil mendapat tempat duduk?"),
                MakeInitiative($"{mode}_child", "Menggendong Anak", NpcRole.CarryingChild, vehicle,
                    $"Penumpang yang sedang menggendong anak kecil terlihat kewalahan berdiri di {vehicle}.",
                    "Apakah anda ingin memberikan tempat duduk?"),
                MakeInitiative($"{mode}_disability", "Penumpang Disabilitas", NpcRole.Disability, vehicle,
                    $"Penumpang dengan disabilitas mencari tempat yang lebih aman untuk duduk di {vehicle}.",
                    "Apakah anda ingin memberikan tempat duduk?"),
                MakeInitiative($"{mode}_elderly", "Lansia", NpcRole.Elderly, vehicle,
                    $"Seorang lansia berdiri tidak jauh darimu di {vehicle}.",
                    "Apakah anda ingin memberikan tempat duduk?"),
            };
        }

        static IncidentDefinition MakePriority(TransportMode mode, string vehicle, string contact, string contactSub)
        {
            var incident = MakeReport(
                id: $"{mode}_priority",
                title: "Kursi Prioritas",
                role: NpcRole.PrioritySeatAbuse,
                vehicle: vehicle,
                contact: contact,
                contactSub: contactSub,
                intro: $"Seseorang terduduk di kursi prioritas padahal terlihat mampu, sementara ibu hamil / lansia berdiri di dekatnya di {vehicle}.",
                correctId: "priority",
                correctLabel: "Lapor: salah duduk di kursi prioritas",
                correctChat: $"Ada orang yang tidak berhak duduk di kursi prioritas di {vehicle}, sementara yang berhak berdiri. Mohon dibantu.",
                escalate: false,
                afterYes: "Tepat. Petugas menegur dan kursi prioritas dikembalikan untuk yang berhak (lansia, ibu hamil, disabilitas, penggendong anak).",
                afterWrong: "Laporan kurang tepat. Edukasi: kursi prioritas punya aturan—laporkan pelanggaran kursi prioritas atau tegur dengan sopan (−10)."
            );
            incident.allowsNegur = true;
            incident.scoreNegur = 10;
            incident.storyAfterNegur =
                "Kamu menegur dengan sopan. Orang tersebut berdiri dan memberi kesempatan kepada yang berhak. Peduli bisa dimulai dari tindakan kecil yang aman.";
            incident.decisionPrompt = "Tegur sendiri atau lapor lewat WhatsApp ke petugas?";
            return incident;
        }

        static IncidentDefinition MakeReport(
            string id, string title, NpcRole role, string vehicle, string contact, string contactSub,
            string intro, string correctId, string correctLabel, string correctChat,
            bool escalate, string afterYes, string afterWrong)
        {
            return new IncidentDefinition
            {
                id = id,
                title = title,
                category = EventCategory.Report,
                npcRole = role,
                introStory = intro,
                decisionPrompt = "Buka WhatsApp dan pilih pesan laporan yang sesuai.",
                timeLimit = 18f,
                scoreYes = 10,
                scoreNo = -20,
                scoreTimeout = -40,
                scoreWrongReport = -10,
                scoreCancel = 0,
                usesWhatsApp = true,
                allowsNegur = false,
                escalateOnCorrect = escalate,
                whatsappContactName = contact,
                contactSubtitle = contactSub,
                storyAfterYes = afterYes,
                storyAfterNo =
                    "Dengan tidak bertindak, masalah bisa berlanjut. Edukasi: hubungi penanggung jawab kendaraan lewat kanal yang aman.",
                storyAfterTimeout =
                    "Waktu habis. Ingat: amati dulu, lalu lapor ke petugas dengan pesan yang sesuai kejadian.",
                storyAfterWrongReport = afterWrong,
                storyAfterCancel =
                    "Kamu menutup chat tanpa mengirim (0 poin). Kalau situasi mengganggu atau berbahaya, kirim laporan yang sesuai ke petugas.",
                reportOptions = BuildOptions(vehicle, correctId, correctLabel, correctChat)
            };
        }

        static List<ReportOption> BuildOptions(string vehicle, string correctId, string correctLabel, string correctChat)
        {
            var all = new List<ReportOption>
            {
                new ReportOption
                {
                    id = "loud",
                    buttonLabel = "Lapor: suara mengganggu",
                    chatPreview = $"Halo, di {vehicle} ada penumpang berteriak sangat keras dan mengganggu. Mohon dibantu tegur.",
                    isCorrect = correctId == "loud"
                },
                new ReportOption
                {
                    id = "priority",
                    buttonLabel = "Lapor: salah duduk di kursi prioritas",
                    chatPreview = $"Ada orang yang tidak berhak duduk di kursi prioritas di {vehicle}. Mohon dibantu.",
                    isCorrect = correctId == "priority"
                },
                new ReportOption
                {
                    id = "phone",
                    buttonLabel = "Lapor: volume HP tanpa earphone",
                    chatPreview = $"Di {vehicle} ada yang nyalakan speaker HP terlalu kencang tanpa earphone.",
                    isCorrect = correctId == "phone"
                },
                new ReportOption
                {
                    id = "harass",
                    buttonLabel = "Lapor: dugaan pelecehan",
                    chatPreview = $"Darurat: dugaan pelecehan / perilaku tidak pantas di {vehicle}. Mohon segera ke lokasi.",
                    isCorrect = correctId == "harass"
                },
                new ReportOption
                {
                    id = "fight",
                    buttonLabel = "Lapor: perkelahian",
                    chatPreview = $"Tolong! Ada orang berantem di {vehicle}. Mohon petugas datang amankan.",
                    isCorrect = correctId == "fight"
                },
                new ReportOption
                {
                    id = "trash",
                    buttonLabel = "Lapor: sampah berserakan",
                    chatPreview = $"Di {vehicle} banyak sampah berserakan di lantai.",
                    isCorrect = false
                }
            };

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].id == correctId)
                {
                    all[i].buttonLabel = correctLabel;
                    all[i].chatPreview = correctChat;
                    all[i].isCorrect = true;
                }
                else
                {
                    all[i].isCorrect = false;
                }
            }

            var chosen = new List<ReportOption>();
            ReportOption correct = null;
            var distractors = new List<ReportOption>();
            foreach (var o in all)
            {
                if (o.isCorrect) correct = o;
                else distractors.Add(o);
            }

            for (int i = distractors.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (distractors[i], distractors[j]) = (distractors[j], distractors[i]);
            }

            chosen.Add(correct);
            for (int i = 0; i < 3 && i < distractors.Count; i++)
                chosen.Add(distractors[i]);

            for (int i = chosen.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (chosen[i], chosen[j]) = (chosen[j], chosen[i]);
            }

            return chosen;
        }

        static IncidentDefinition MakeInitiative(string id, string title, NpcRole role, string vehicle, string intro, string prompt)
        {
            return new IncidentDefinition
            {
                id = id,
                title = title,
                category = EventCategory.Initiative,
                npcRole = role,
                introStory = intro,
                decisionPrompt = prompt,
                timeLimit = 12f,
                scoreYes = 10,
                scoreNo = -30,
                scoreTimeout = -50,
                usesWhatsApp = false,
                allowsNegur = false,
                storyAfterYes =
                    "Inisiatif bagus! Memberi tempat duduk adalah bentuk kepedulian sederhana yang sangat berarti.",
                storyAfterNo =
                    "Kelompok rentan membutuhkan kenyamanan lebih. Tawarkan kursi bila kamu mampu.",
                storyAfterTimeout =
                    "Edukasi: utamakan ibu hamil, lansia, disabilitas, dan yang menggenggam anak kecil untuk duduk."
            };
        }
    }
}
