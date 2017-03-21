using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServices
{
	public class Service
	{
		public static Service Instance { get; } = new Service();

		public Service()
		{
			FaceServiceClient = new FaceServiceClient("d5a0498b3ee44d79ac91028723e0a1a9");
			_personGroupId = Guid.NewGuid().ToString();
		}

		public List<Person> People { get; } = new List<Person>
		{
			new Person{
				Name = "Alejandro Ruiz",
				PhotoUrl = "https://scontent.fgdl4-1.fna.fbcdn.net/v/t1.0-9/12592451_1082742655104204_5022196324063214989_n.jpg?oh=87e5fb9fc13b227c55530a24e35550d5&oe=58FF1BD2",
				City = "Guadalajara"
			},
			new Person{
				Name = "Enrique Aguilar",
				PhotoUrl = "https://4.bp.blogspot.com/-LWenLgQMTxg/Vu7SeDxXO7I/AAAAAAAADkg/9O42DFTU3KE-cf-3THpZuIonJoQOpXTNw/s1600/eavmvp.png",
				City = "Leon"
			},
			new Person{
				Name = "Humberto Jaimes",
				PhotoUrl = "https://media.licdn.com/mpr/mpr/shrinknp_200_200/p/8/005/09b/2df/14c8a73.jpg",
				City = "Mexico City"
			}
		};

		string _personGroupId;

		public FaceServiceClient FaceServiceClient { get; private set; }

		public async Task RegisterEmployees()
		{
			await FaceServiceClient.CreatePersonGroupAsync(_personGroupId, "MVPs In Mexico");

			foreach (var xmvp in People)
			{
				var p = await FaceServiceClient.CreatePersonAsync(_personGroupId, xmvp.Name);
				await FaceServiceClient.AddPersonFaceAsync(_personGroupId, p.PersonId, xmvp.PhotoUrl);
				xmvp.GroupId = _personGroupId;
				xmvp.PersonId = p.PersonId.ToString();
			}

			await TrainPersonGroup();
		}

		public async Task<Person> FindSimilarFace(Stream faceData)
		{
			var faces = await FaceServiceClient.DetectAsync(faceData);
			var faceIds = faces.Select(face => face.FaceId).ToArray();

			var results = await FaceServiceClient.IdentifyAsync(_personGroupId, faceIds);
			var result = results[0].Candidates[0].PersonId;

			var person = await FaceServiceClient.GetPersonAsync(_personGroupId, result);
			return new Person
			{
				Name = person.Name,
				PersonId = result.ToString()
			};
		}

		public async Task<bool> AddFace(Stream faceData, Person person)
		{
			try
			{
				var result = await FaceServiceClient.AddPersonFaceAsync(person.GroupId, Guid.Parse(person.PersonId), faceData);
				if (result == null || string.IsNullOrWhiteSpace(result.PersistedFaceId.ToString()))
					return false;
				return true;
			}
			catch
			{
				return false;
			}
		}

		public async Task TrainPersonGroup()
		{
			try
			{
				await FaceServiceClient.TrainPersonGroupAsync(_personGroupId);
				TrainingStatus trainingStatus = null;
				while (true)
				{
					trainingStatus = await FaceServiceClient.GetPersonGroupTrainingStatusAsync(_personGroupId);

					if (trainingStatus.Status != Status.Running)
					{
						break;
					}

					await Task.Delay(1000);
				}
				return;
			}
			catch
			{
				return;
			}
		}

		public async Task<Face> AnalyzeFace(Stream faceData)
		{
			var faces = await FaceServiceClient.DetectAsync(faceData, false, false, new List<FaceAttributeType> {
				FaceAttributeType.Age,
				FaceAttributeType.FacialHair,
				FaceAttributeType.Gender,
				FaceAttributeType.Glasses,
				FaceAttributeType.HeadPose,
				FaceAttributeType.Smile
			});
			if (faces.Length > 0)
				return faces[0];
			return null;
		}
	}
}
