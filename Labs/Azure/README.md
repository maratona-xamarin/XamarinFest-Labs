## Xamarin Fest Hands On Lab - Azure

Aviso: Inicialmente o material foi criado em inglês, mas será traduzido em breve :)


IMPORTANT: This guide was inspired in step by step of Xamarin Dev Days, created by James Montemagno.

Today, we will be building a [Xamarin.Forms](http://xamarin.com/forms) application using Azure platform to store data in the cloud. This application will display a list of attendees at Xamarin Fest and show their details. We will start by building some business logic backend that pulls down json from a RESTful endpoint and then we will connect it to an Azure Mobile App backend in just a few lines of code. We will also use Blob Storage to save profile photos of attendees.


### Get Started

Open **Start/Attendees/Attendees.sln**

This solution contains 4 projects

* Core  - PCL project that will have all shared code (model, views, and view models).
* Droid - Xamarin.Android application
* iOS - Xamarin.iOS application

![Solution](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/2a6c402b-8901-4b0c-8945-4f9bf13bc891/visual-studio-projeto.PNG)

The **Attendees** project also has blank code files and XAML pages that we will use during the Hands on Lab.

#### NuGet Restore

All projects have the required NuGet packages already installed, so there will be no need to install additional packages during the Hands on Lab. The first thing that we must do is restore all of the NuGet packages from the internet.

This can be done by **Right-clicking** on the **Solution** and clicking on **Restore NuGet packages...**

If you use Xamarin Studio in the Mac, probably the packages are restored automatically.

![Restore NuGets](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/d0a169ba-150b-4017-bea0-92ad9ed402b9/visual-studio-nuget-restore.png)

### Model

We will be pulling down information about attendees. Open the **Attendees/Models/AttendeeModel.cs** file and add the following properties inside of the **AttendeeModel** class:

```csharp
public string Id { get; set; }
public string Name { get; set; }
public string Email { get; set; }
public string PhotoName { get; set; }
```

### View Model

The **AttendeesViewModel.cs** will provide all of the functionality for how our main Xamarin.Forms view will display data in attendees list page. It will consist of a list of attendees and a method that can be called to get the attendees from the server.

#### Implementing INotifyPropertyChanged

*INotifyPropertyChanged* is important for data binding in MVVM Frameworks. This is an interface that, when implemented, lets our view know about changes to the model.

By default, every own viewmodels will be inherit from **BaseViewModel**. So, we will implement **INotifyPropertyChanged** in base class.

Update:

```csharp
public abstract class BaseViewModel
{
}
```

to

```csharp
public abstract class BaseViewModel : INotifyPropertyChanged
{
}
```


Simply right click and tap **Implement Interface**, which will add the following line of code:

```csharp
public event PropertyChangedEventHandler PropertyChanged;
```

We will code a helper method named **OnPropertyChanged** that will raise the **PropertyChanged** event (see below). We will invoke the helper method whenever a property changes.

##### C# 6 (Visual Studio 2015 or Xamarin Studio on Mac)
```csharp
private void OnPropertyChanged([CallerMemberName] string name = null) =>
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
```


##### C# 5 (Visual Studio 2012 or 2013)
```csharp
private void OnPropertyChanged([CallerMemberName] string name = null)
{
    var changed = PropertyChanged;

    if (changed == null)
       return;

    changed.Invoke(this, new PropertyChangedEventArgs(name));
}
```

Now, we can call **OnPropertyChanged();** whenever a property updates. Let's create our first property now.

#### IsBusy
We will create a backing field and accessors for a boolean property. This will let our view know that our view model is busy so we don't perform duplicate operations (like allowing the user to refresh the data multiple times).

First, create the backing field:

```csharp
private bool _isBusy;
```

Next, create the property:

```csharp
public bool IsBusy
{
    get { return _isBusy; }
    set
    {
        _isBusy = value;
        OnPropertyChanged();
    }
}
```

Notice that we call **OnPropertyChanged();** whenever the value changes. The Xamarin.Forms binding infrastructure will subscribe to our **PropertyChanged** event so the UI will get notified of the change.

For finish, we will update **AttendeesViewModel** to inherit **BaseViewModel** class.

Update:

```csharp
public class AttendeesViewModel
{

}
```

to

```csharp
public class AttendeesViewModel : BaseViewModel
{

}
```


#### ObservableCollection of AttendeeModel

We will use an **ObservableCollection<AttendeeModel>** that will be cleared and then loaded with attendees. We will use an **ObservableCollection** because it has built-in support for **CollectionChanged** events when we Add or Remove from it. This is very nice so we don't have to call **OnPropertyChanged** each time.

Above the constructor of the AttendeesViewModel class definition, declare an auto-property:

```csharp
public ObservableCollection<AttendeeModel> Attendees { get; set; }
```

Inside of the constructor, create a new instance of the `ObservableCollection`:

```csharp
public AttendeeViewModel()
{
    Attendees = new ObservableCollection<AttendeeModel>();
}
```

#### GetAttendees Method

We are now set to create a method named **GetAttendees** which will retrieve the attendee data from the internet. We will first implement this with a simply HTTP request, but later update it to grab and sync the data from Azure!

First, create a method called **GetAttendees** which is of type *async Task* (it is a Task because it is using Async methods):

```csharp
private async Task GetAttendees()
{

}
```
The following code will be written INSIDE of this method:

First is to check if we are already grabbing data:

```csharp
private async Task GetAttendees()
{
    if (IsBusy)
        return;
}
```

Next we will create some scaffolding for try/catch/finally blocks:

```csharp
private async Task GetAttendees()
{
    if (IsBusy)
        return;

    Exception error = null;

    try
    {
        IsBusy = true;

    }
    catch (Exception ex)
    {
        error = ex;
    }
    finally
    {
       IsBusy = false;
    }
}
```

Notice, that we set *IsBusy* to true and then false when we start to call to the server and when we finish.

Now, we will use *HttpClient* to grab the json from the server inside of the **try** block.

 ```csharp
using(var client = new HttpClient())
{
    // Grab json from the server
    var json = await client.GetStringAsync("https://demo8270147.mockable.io/attendees");
}
```

Still inside of the **using**, we will Deserialize the json and turn it into a list of Attendees with Json.NET:

```csharp
var items = JsonConvert.DeserializeObject<List<AttendeeModel>>(json);
```

Still inside of the **using**, we will clear the attendees and then load them into the ObservableCollection:

```csharp
Attendees.Clear();
foreach (var item in items)
    Attendees.Add(item);
```
If anything goes wrong the **catch** will save the exception and AFTER the finally block we can pop up an alert:

```csharp
if (error != null)
    await MessageHelper.Instance.ShowMessage("Error!", error.Message, "OK");
```

The completed code should look like:

```csharp
private async Task GetAttendees()
{
    if (IsBusy)
        return;

    Exception error = null;

    try
    {
        IsBusy = true;

        using(var client = new HttpClient())
        {
            //grab json from server
            var json = await client.GetStringAsync("https://demo8270147.mockable.io/attendees");

            //Deserialize json
            var items = JsonConvert.DeserializeObject<List<AttendeeModel>>(json);

            //Load attendees into list
            Attendees.Clear();
            foreach (var item in items)
                Attendees.Add(item);
        }
    }
    catch (Exception e)
    {
        LogHelper.Instance.AddLog(e);
        error = e;
    }
    finally
    {
        IsBusy = false;
    }

    if (error != null)
        await MessageHelper.Instance.ShowMessage("Error!", error.Message, "OK");
}
```

Our main method for getting data is now complete!

#### GetAttendees Command

Instead of invoking this method directly, we will expose it with a **Command**. A Command has an interface that knows what method to invoke and has an optional way of describing if the Command is enabled.

Create a new Command called **GetAttendeesCommand**:

```csharp
public Command GetAttendeesCommand { get; set; }
```

Inside of the `AttendeesViewModel` constructor, create the `GetAttendeesCommand` and pass it one method to invoke when the command is executed:

```csharp
GetAttendeesCommand = new Command(
	async () => await GetAttendees()
);
```

## The User Interface
It is now finally time to build out our first Xamarin.Forms user interface in the **Views/AttendeesView.xaml**

### AttendeesView.xaml

For the first page we will add a few vertically-stacked controls to the page. We can use a StackLayout to do this. Between the `ContentPage` tags add the following:

```xml
 <StackLayout>

  </StackLayout>
```

This will be the container where all of the child controls will go.

Next, let's add a Button that has a binding to the **LoadAttendeesCommand** that we created (see below). The command takes the place of a clicked handler and will be executed whenever the user taps the button.

```xml
<Button Text="Sync Attendees" Command="{Binding GetAttendeesCommand}"/>
```

Under the button we can display a loading bar when we are gathering data from the server. We can use an ActivityIndicator to do this and bind to the IsBusy property we created:

```xml
<ActivityIndicator IsRunning="{Binding IsBusy}" IsVisible="{Binding IsBusy}"/>
```

We will use a ListView that binds to the Attendees collection to display all of the items:

```xml
<ListView
    ItemsSource="{Binding Attendees}">
        <!--Add ItemTemplate Here-->
</ListView>
```

We still need to describe what each item looks like, and to do so, we can use an ItemTemplate that has a DataTemplate with a specific View inside of it. Xamarin.Forms contains a few default Cells that we can use, and we will use the **TextCell** that has two rows of text.

Replace <!--Add ItemTemplate Here--> with:

```xml
<ListView.ItemTemplate>
    <DataTemplate>
        <TextCell
            Text="{Binding Name}"
            Detail="{Binding Email}" />
    </DataTemplate>
</ListView.ItemTemplate>
```

### Connect the View with the ViewModel
As we have bound some elements of the View to ViewModel properties, we have to tell the View now, which ViewModel to bind against. For this, we have to set the `BindingContext` to the `AttendeesViewModel`, we created. Open the `AttendeesView.xaml.cs` file and see, that we already did this binding for you.

```csharp
private AttendeesViewModel _viewModel;

public AttendeesView()
{
    InitializeComponent();

    // Create the view model and set as binding context
    _viewModel = new AttendeesViewModel();
    BindingContext = _viewModel;
}
```

### Validate App.cs

Open the App.cs file and you will see the entry point for the application, which is the constructor for `App()`. It simply creates the  AttendeesView, and then wraps it in a navigation page to get a nice title bar.

### Run the App!

Set the iOS, Android, or UWP (Windows/VS2015 only) as the startup project and start debugging.

![Startup project](http://content.screencast.com/users/JamesMontemagno/folders/Jing/media/020972ff-2a81-48f1-bbc7-1e4b89794369/2016-07-11_1442.png)

#### iOS
If you are on a PC then you will need to be connected to a macOS device with Xamarin installed to run and debug the app.

If connected, you will see a Green connection status. Select `iPhoneSimulator` as your target, and then select the Simulator to debug on.

![iOS Setup](http://content.screencast.com/users/JamesMontemagno/folders/Jing/media/a6b32d62-cd3d-41ea-bd16-1bcc1fbe1f9d/2016-07-11_1445.png)

#### Android

Simply set the Core.Droid as the startup project and select a simulator to run on. The first compile may take some additional time as Support Packages are downloaded, so please be patient.

If you run into an issue building the project with an error such as:

**aapt.exe exited with code** or **Unsupported major.minor version 52** then your Java JDK may not be setup correctly, or you have newer build tools installed then what is supported. See this technical bulletin for support: https://releases.xamarin.com/technical-bulletin-android-sdk-build-tools-24/

Additionally, see James' blog for visual reference: http://motzcod.es/post/149717060272/fix-for-unsupported-majorminor-version-520

If you are running into issues with Android support packages that can't be unzipped because of corruption please check: https://xamarinhelp.com/debugging-xamarin-android-build-and-deployment-failures/


## Details

Now, let's do some navigation and display some Details. Let's open up the code-behind for **AttendeesView.xaml** called **AttendeesView.xaml.cs**.

### ItemTapped Event

Now, we add an event in our listview. In **AttendeesView.xaml** we add **ItemTapped** event.

```xml
<ListView
    ItemsSource="{Binding Attendees}"
    ItemTapped="OnItemTapped">
        <!--Add ItemTemplate Here-->
</ListView>
```

In the code-behind you will declare **OnItemTapped** event to receive data. The page is **AttendeesView.xaml.cs**. Implement this method so it navigates to the DetailsView passing AttendeeModel object tapped:

```csharp
private async void OnItemTapped(object sender, ItemTappedEventArgs e)
{
    (sender as ListView).SelectedItem = null;
    var attendeeModel = e.Item as AttendeeModel;
	await NavigationHelper.Instance.GotoDetails(attendeeModel);
}
```

In the above code we check to see if the tapped item is not null and then use the built in **Navigation** API to push a new page and then deselect the item.

Now, we need prepare the view and viewmodel of details page. Lets get started with **DetailsView.xaml.cs**.

### DetailsView.xaml.cs

When we tapped in listview item, we called details page and pass attendee object as parameter. We receive this parameter in **DetailsView.xaml.cs**. We need pass this parameter to a **DetailsViewModel**. Lets go.

```csharp
public partial class DetailsView : ContentPage
{
	public DetailsView(AttendeeModel attendeeModel)
	{
		InitializeComponent();
		BindingContext = new DetailsViewModel(attendeeModel);
	}
}
```

### DetailsViewModel.cs

In details view model, we need inherited of **BaseViewModel** for this class implement **INotifyPropertyChanged** event.

```csharp
public class DetailsViewModel : BaseViewModel
{
}
```

Declare a **AttendeeModel** property. With this property we will display data in the view and edit this later.

```csharp
public class DetailsViewModel : BaseViewModel
{
	private AttendeeModel _attendee;
	public AttendeeModel Attendee
	{
		get { return _attendee; }
		set
		{
			_attendee = value;
			OnPropertyChanged();
		}
    }
}
```

In constructor, we receive attendee object and assign to a property.

```csharp
public DetailsViewModel(AttendeeModel attendeeModel)
{
	Attendee = attendeeModel;
}
```

### DetailsView.xaml

Let's now fill in the DetailsView. Similar to the AttendeesView, we will use a StackLayout, but we will wrap it in a ScrollView in case we have long text.

```xml
<ScrollView Padding="10">
    <StackLayout Spacing="10">
        <!-- Detail controls here -->
    </StackLayout>    
</ScrollView>
```

Now, let's add controls and bindings for the properties in the Attendee object:

```xml
<Entry
    Text="{Binding Attendee.Name}"
    Placeholder="Nome"
/>

<Entry
    Text="{Binding Attendee.Email}"
    Placeholder="Email"
/>
```

Entry fields permit us to change inserted values. We not implement this now, we will in the next chapter.


### Compile & Run
Now, we should be all set to compile and run just like before!

## Connect to Azure Mobile Apps

Of course being able grab data from a RESTful end point is great, but what about a full back end? This is where Azure Mobile Apps comes in. Let's upgrade our application to use an Azure Mobile Apps back end.

Head to [http://portal.azure.com](http://portal.azure.com) and register for an account.

Once you are in the portal select the **+ New** button and search for **mobile apps** and you will see the results as shown below. Select **Mobile Apps Quickstart**

![Quickstart](http://content.screencast.com/users/JamesMontemagno/folders/Jing/media/c2894f06-c688-43ad-b812-6384b34c5cb0/2016-07-11_1546.png)

The Quickstart blade will open, select **Create**

![Create quickstart](http://content.screencast.com/users/JamesMontemagno/folders/Jing/media/344d6fc2-1771-4cb7-a49a-6bd9e9579ba6/2016-07-11_1548.png)

This will open a settings blade with 4 settings:

**App name**

This is a unique name for the app that you will need when you configure the back end in your app. You will need to choose a globally-unique name; for example, you could try something like *xamarinfestattendees*.

**Subscription**
Select a subscription or create a pay-as-you-go account (this service will not cost you anything)

**Resource Group**
Select *Create new* and call it **XamarinFestAttendees**

A resource group is a group of related services that can be easily deleted later.

**App Service plan/Location**
Click this field and select **Create New**, give it a unique name, select a location (typically you would choose a location close to your customers), and then select the F1 Free tier:

![service plan](http://content.screencast.com/users/JamesMontemagno/folders/Jing/media/7559d3f1-7ee6-490f-ac5e-d1028feba88f/2016-07-11_1553.png)

Finally check **Pin to dashboard** and click create:

![](http://content.screencast.com/users/JamesMontemagno/folders/Jing/media/a844c283-550c-4647-82d3-32d8bda4282f/2016-07-11_1554.png)

This will take about 3-5 minutes to setup, so let's head back to the code!


When the Quickstart finishes you should see the following screen, or can go to it by tapping the pin on the dashboard. Select **Easy Tables** option.

![Quickstart](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/c1ddb9a0-2ce9-4c28-81d7-644831aec09c/mobile-apps-quickstart-menu-easy-tables.png)

It will have created a `TodoItem`, which you should see, but we can create a new table. Just click in **Add** button, put name of new table and click **Ok**. Our new table is call **attendees**.

![Add table](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/e84c9db4-3108-486c-9a65-aa1d2c7bcc41/add-table.png)

With new table, we need add some columns for populate data inside database. Just click in a table and **Manager schema** button.

![Manage schema](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/47b0f458-8339-4530-9a85-4aa6f3559dcc/manage-schema.png)

When schema is opened you can see default columns of this table. We will create new columns. Click **Add a column**, put a name and click **Ok**. We will create three columns: name, email and photo_name.

![Add column](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/f1d12fd8-5c91-4c8e-920e-aca87f6ba44d/add-field.png)

Nice! We have a REST API working. You can test using curl or Postman, will works!

Now, go back to Xamarin app and implement Azure Mobile Client.

### Updating AttendeeModel

We create a new table with some fields, ok? But, how Azure Mobile SDK know tables and fields to load and save data in the app? Its important to mapping the model with table name and fields created. Lets go:

update

```csharp
public class AttendeeModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhotoName { get; set; }

    [Version]
    public string Version { get; set; } // no change this, is important for Azure
}
```

to

```csharp
[DataTable("attendees")]
public class AttendeeModel
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("photo_name")]
    public string PhotoName { get; set; }

    [Version]
    public string Version { get; set; } // no change this, is important for Azure
}
```

### Database name and mobile API URI

In the project, exists a static class called **AppConfig**. We will configure **DatabaseName** and **MobileAppUri** with information about the service created. Update this values with information of your service.

```csharp
public static class AppConfig
{
	public static string DatabaseName = "attendees";
	public static string MobileAppUri = "https://OUR-APP-NAME-HERE.azurewebsites.net";
	public static string StorageAppConfig = "";
}
```

This is an elegant way to centralize all configuration of your app.

### Update AzureMobileService.cs
We will be using the [Azure Mobile Apps SDK](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-xamarin-forms-get-started/) to add an Azure back end to our mobile app in just a few lines of code.

In the Core/Services/AzureMobileService.cs file exists the **Initializer**. The Initialize logic will setup our database and create our `IMobileServiceSyncTable<AttendeeModel>` table that we can use to get attendee data from Azure. There are just two methods that we need to fill in to get and sync data from the server.

Pay attention, **AppConfig.DatabaseName** and **AppConfig.MobileAppUri** configured above is referenced in this method for make configuration correctly.

Before execute any operations with this SDK, is important call **Initialize** method to ensure that all is configured.

#### GetAttendees
In this method we will need to Initialize, Sync, and query the table for items. We can use complex linq queries to order the results:

```csharp
public async Task<IList<AttendeeModel>> GetAttendees()
{
	await Initialize();
	await SyncAttendees();
	return await _attendee.OrderBy(a => a.Name).ToListAsync();
}
```

#### SyncAttendees
Our azure backend has the ability to push any local changes and then pull all of the latest data from the server using the following code that can be added to the try inside of the SyncAttendees method:

```csharp
private async Task SyncAttendees()
{
	try
	{
		await _client.SyncContext.PushAsync();
		await _attendee.PullAsync("attendees", _attendee.CreateQuery());
	}
	catch (Exception e)
	{
		LogHelper.Instance.AddLog(e);
	}
}
```
That is it for our Azure code! Just a few lines of code, and we are ready to grat the data from azure.

### Update AttendeesViewModel.cs

Update async Task GetAttendees():

Now, instead of using the HttpClient to get a string, let's query the Table:

Change the *try* block of code to:

```csharp
public async Task GetAttendees()
{
	if (IsBusy)
		return;

	try
	{
		IsBusy = true;
        var items = await AzureMobileService.Instance.GetAttendees();

		Attendees.Clear();
        foreach (var item in items)
    		Attendees.Add(item);
	}
	catch (Exception e)
	{
		LogHelper.Instance.AddLog(e);
	}
	finally
	{
		IsBusy = false;
	}
}
```

Now, we have implemented all of the code we need in our app! Amazing isn't it! That's it! App Service will automatically handle all communication with your Azure back end for you, do online/offline synchronization so your app works even when it's not connected.


Try re-run your application and get data from Azure!

Lets continue our implementation. Now, we implement add, delete and update capabilities in our app. Lets go!

### Add new attendee

For this, we need add a button in attendees page and call a command for open detail page. For this, lets add a button in a toolbar item of the app. Open **AttendeesView.xaml** and put the following content inside **ContentPage**.

```xml
<ContentPage
...
>
	<ContentPage.ToolbarItems>
		<ToolbarItem
			Text="Add"
			Command="{Binding AddAttendeeCommand}"
		/>
	</ContentPage.ToolbarItems>

    <ContentPage.Content>
    	<!-- ... -->
    </ContentPage.Content>
</ContentPage>
```

In **AttendeesViewModel**, add new command and open detail page when command execute.

```csharp
public class AttendeesViewModel : BaseViewModel
{
	public ICommand AddAttendeeCommand
			=> new Command(async () => await AddAttendee());

    private async Task AddAttendee()
	{
		await NavigationHelper.Instance.GotoDetails(new AttendeeModel());
	}
}
```

You can run the app and see navigate to another page when click in add button. Try!

Now, we need implement in details page a button to call save action and a command in viewmodel to execute operation. Open **DetailsView.xaml** and add toolbar item save button:

```xml
<ContentPage
...
>
	<ContentPage.ToolbarItems>
		<ToolbarItem
			Text="Save"
			Command="{Binding SaveCommand}"
		/>
	</ContentPage.ToolbarItems>

    <ContentPage.Content>
    	<!-- ... -->
    </ContentPage.Content>
</ContentPage>
```

Lets start implement **SaveCommand** in **DetailsViewModel.cs**

```csharp
public class DetailsViewModel : BaseViewModel
{
	public ICommand SaveCommand
		=> new Command(async () => await Save());

    private async Task Save()
    {
    	if (IsBusy)
			return;

    	Exception exception = null;

    	try
    	{
    		IsBusy = true;

            if (string.IsNullOrEmpty(Attendee.Name))
				throw new Exception("The name is required");

			if (string.IsNullOrEmpty(Attendee.Email))
				throw new Exception("The e-mail is required");

            await AzureMobileService.Instance.SaveAttendee(Attendee);
    	}
    	catch (Exception e)
    	{
    		exception = e;
			LogHelper.Instance.AddLog(e);
    	}
    	finally
    	{
	    	IsBusy = false;
    	}

        if (exception != null)
		{
			await MessageHelper.Instance.ShowMessage(
				"An error has occurred",
				exception.Message,
				"Ok"
			);
			return;
		}

		await NavigationHelper.Instance.GoBack();
    }
}
```

For finish, we need implement **SaveAttendee** method located in **AzureMobileService.cs** file.

```csharp
public async Task SaveAttendee(AttendeeModel attendeeModel)
{
	await Initialize();

	if (string.IsNullOrEmpty(attendeeModel.Id))
		await _attendee.InsertAsync(attendeeModel);
	else
		await _attendee.UpdateAsync(attendeeModel);

	await SyncAttendees();
}
```

### Edit an attendee

For edit an attendee, you just only select an item of listivew and change data. The data persistence is already configured in previous section.

### Delete an attendee

For this, we need add a button for delete in details page, add a command for binding with button and implement Azure Service for delete data.

Open **DetailsView.xaml** and add a button entries.

```xml
<StackLayout>
	<Entry
		Text="{Binding Attendee.Name}"
		Placeholder="Name"
	/>

	<Entry
		Text="{Binding Attendee.Email}"
		Placeholder="E-mail"
	/>

    <Button
		Text="Delete"
		Command="{Binding DeleteCommand}"
    />
</StackLayout>
```

Add a command in **DetailsViewModel**.

```csharp
public class DetailsViewModel : BaseViewModel
{
	public ICommand DeleteCommand
		=> new Command(async () => await Delete());

    private async Task Delete()
    {
    	if (IsBusy)
			return;

        if (string.IsNullOrEmpty(Attendee.Id))
		    return;

        var delete = await MessageHelper.Instance.ShowAsk(
			"Delete attendee",
			"You sure delete this attendee?",
			"Yes",
			"No"
		);

        if (delete == false)
			return;

        Exception exception = null;

        try
		{
			IsBusy = true;
			await AzureMobileService.Instance.DeleteAttendee(Attendee);
		}
        catch (Exception e)
		{
			exception = e;
			LogHelper.Instance.AddLog(e);
		}
        finally
		{
			IsBusy = false;
		}

        if (exception != null)
		{
			await MessageHelper.Instance.ShowMessage(
				"An error has ocurred",
				exception.Message,
				"Ok"
			);
			return;
		}

		await NavigationHelper.Instance.GoBack();
    }
}
```

To finish, we need implement delete command in **AzureMobileService.cs** class:

```csharp
public async Task DeleteAttendee(AttendeeModel attendeeModel)
{
	await Initialize();
    await _attendee.DeleteAsync(attendeeModel);
    await SyncAttendees();
}
```

Congratulations! You finish CRUD operations!

## Connect to Azure Blob Storage

For use Azure Blob Store you need Windows Azure Storage SDK to perform operations. With this SDK, you can upload, download and delete files in the Azure Blob Storage. In our app, we take photo profile for attendee and use SDK to send data to the cloud.

### Prepare Azure
Go back to Azure and search for Storage account:

![Search storage account](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/6a5b9de8-0813-4ca3-85ff-d0574ff6c6b2/search-storage-account.png)

After select storage account, confirm creation:

![Create storage account](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/7ee82927-19e4-4f70-aa76-547afd077a5e/create-storage-account.png)

For finish, insert basic information about your storage account.
Pay attention in the name of your storage account. You need this for configure app integration. The name used in the sample is **xamarinfesttest**. Select account king to blob storage and existing resource group created recently.

![Create storage account - finish](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/b325b1f9-2aa2-4a5f-acad-c5928ac75ed3/configure-storage-account.png)

After storage account created, you need configure account key in you app.

We need provide some informations in account key for configure app:

"DefaultEndpointsProtocol=**PROTOCOL**;AccountName=**ACCOUNT_NAME**;AccountKey=**ACCOUNT_KEY**"

* Protocol: The protocol of http requests ( http|https )
* Account name: is the name of your storage account
* Account key: for get a key, click in key icon and copy one of keys available.

See below:

![Get key](https://content.screencast.com/users/ionixjunior/folders/XamarinFest/media/9f7879a9-f09c-4516-94db-cc836cd4f038/configure-keys.png)

With this connection string, you can update the attribute **StorageAppConfig** of the **AppConfig.cs** class in app.

The Xamarin documentation provide a full explanation for this. See https://developer.xamarin.com/guides/xamarin-forms/cloud-services/storage/azure-storage/

### Implementation in app

Ok, we have a storage account configured and we need add this capabilities in our app.
An important thing about this is Azure Blob Storage is not equals Azure Mobile App. Azure Blob Storage not provide a offline capabilities to storage data in device and synchronize when turn online. For this implementation, we consider stay online for perform operations, ok? Lets go.

First, add new image element in details page above of the entries.

```xml
<StackLayout>
	<Image Source="{Binding Photo}" HeightRequest="80">
    	<Image.GestureRecognizers>
        	<TapGestureRecognizer
				Command="{Binding ChangePhotoCommand}"
			/>
        </Image.GestureRecognizers>
    </Image>

    <Entry ... />
    <Entry ... />
</StackLayout>
```

This image have a Photo property binded and a command to change photo when image was tapped. Create this property and command in details viewmodel.

```csharp
private object _photo;
public object Photo
{
	get { return _photo; }
	set
	{
		_photo = value;
		OnPropertyChanged();
	}
}
```

In constructor assign the value profile.png to a Photo property by default. This image is present in plattform projects and is the default, when not exists a real image to put.

```csharp
public DetailsViewModel(AttendeeModel attendeeModel)
{
	Photo = "profile.png";
}
```

With this, the page will appear and show profile image. Now, lets implement change photo command:

```csharp
public ICommand ChangePhotoCommand => new Command(
	async () => await ChangePhoto()
);

private async Task ChangePhoto()
{
	Exception exception = null;

	try
    {
    	// implementation
    }
    catch (Exception e)
	{
		exception = e;
		LogHelper.Instance.AddLog(e);
	}

    if (exception != null)
	{
		await MessageHelper.Instance.ShowMessage(
			"Something is wrong",
			exception.Message,
			"Ok"
		);
		return;
	}
}
```

In **ChangePhoto** method we need call camera or gallery of the device to select or take a photo. For this, we use a media plugin. Already exists an class in this app for access this features. So, the first thing is ask for the user what to want to do.

```csharp
private async Task ChangePhoto()
{
	var textTakePhoto = "Take photo";
	var textOpenGallery = "Open gallery";
	var textCancel = "Cancel";
	var textDelete = "Delete";

	var actions = new string[] { textTakePhoto, textOpenGallery };

	var response = await MessageHelper.Instance.ShowOptions(
		"What to want to do?",
		textCancel,
		textDelete,
		actions
	);

    if (response == textCancel)
		return;

	Exception exception = null;

    try
    {
    	...
    }
    ...
}
```

If you not select cancel option, the code continues. We need detect what the user selected for perform action.

```csharp
try
{
	if (response == textOpenGallery)
	{
		if (await MediaService.Instance.IsPickPhotoSupported() == false)
			throw new Exception("Is not possible open image gallery");

		var file = await MediaService.Instance.PickPhotoAsync();
		if (file != null)
		{
			Photo = ImageSource.FromFile(file.Path);
			_photoStream = file.GetStream();
		}
	}

	if (response == textTakePhoto)
	{
		if (await MediaService.Instance.IsCameraAvailable() == false)
			throw new Exception("Camera is not available on your device");

		var file = await MediaService.Instance.TakePhotoAsync();
		if (file != null)
		{
			Photo = ImageSource.FromFile(file.Path);
			_photoStream = file.GetStream();
		}
	}

	if (response == textDelete)
	{
		await AzureStorageService.Instance.DeleteFile(Attendee.PhotoName);
		Attendee.PhotoName = null;
		Photo = "profile.png";
	}
}
catch (Exception e)
{
	exception = e;
	LogHelper.Instance.AddLog(e);
}
```

One more thing: in the above code the photo from camera or gallery is assign to **Photo** property to display in the view, but for push to Azure, we need of stream of file. So, for this, we create a new property in this class to store stream of the photo, and assign in **_photoStream** to use later.

```csharp
private Stream _photoStream;
```

After this, we need implement **AzureStorageService.cs** to perform operations in Azure.

In this file, the constructor create a storage account instance and get a reference for image container. Exists more three methods: UploadFile, DownloadFile and DeleteFile.

```csharp
public async Task UploadFile(Stream stream, string name)
{
	await ImageContainer.CreateIfNotExistsAsync();
	var blob = ImageContainer.GetBlockBlobReference(name);
	await blob.UploadFromStreamAsync(stream);
}

public async Task<byte[]> DownloadFile(string name)
{
	await ImageContainer.CreateIfNotExistsAsync();
	var blob = ImageContainer.GetBlobReference(name);
	if (await blob.ExistsAsync())
	{
		await blob.FetchAttributesAsync();
		byte[] bytes = new byte[blob.Properties.Length];
		await blob.DownloadToByteArrayAsync(bytes, 0);
		return bytes;
	}

	return null;
}

public async Task<bool> DeleteFile(string name)
{
	await ImageContainer.CreateIfNotExistsAsync();
	var blob = ImageContainer.GetBlobReference(name);
	return await blob.DeleteIfExistsAsync();
}
```

Ok, nice. Now, when we tapped in save button, we need send this image to Azure Blog Storage, if this image exists. Update **Save** method:

```csharp
try
{
	IsBusy = true;

	if (string.IsNullOrEmpty(Attendee.Name))
		throw new Exception("The name is required");

	if (string.IsNullOrEmpty(Attendee.Email))
		throw new Exception("The e-mail is required");

	await AzureMobileService.Instance.SaveAttendee(Attendee);
}
catch (Exception e)
{
...
}
```

to

```csharp
try
{
	IsBusy = true;

	if (string.IsNullOrEmpty(Attendee.Name))
		throw new Exception("The name is required");

	if (string.IsNullOrEmpty(Attendee.Email))
		throw new Exception("The e-mail is required");

	if (_photoStream != null)
	{
		if (string.IsNullOrEmpty(Attendee.PhotoName))
			Attendee.PhotoName = Guid.NewGuid().ToString();

		await AzureStorageService.Instance.UploadFile(_photoStream, Attendee.PhotoName);
	}

	await AzureMobileService.Instance.SaveAttendee(Attendee);
}
catch (Exception e)
{
...
}
```

With this, when save photo and exists a image, the **photo_name** field is populated with reference of the name of image.

Now, the last implemention we need to do is load images when exists.

Create a command called **GetPhotoCommand** to added in this viewmodel and call him in constructor.

```csharp
public ICommand GetPhotoCommand => new Command(
	async () => await GetPhoto()
);

public DetailsViewModel(AttendeeModel attendeeModel)
{
	Photo = "profile.png";
    GetPhotoCommand.Execute(null);
}

private async Task GetPhoto()
{
	if (IsBusy)
		return;

	if (string.IsNullOrEmpty(Attendee.PhotoName))
		return;

	try
	{
		IsBusy = true;
		var bytes = await AzureStorageService.Instance.DownloadFile(Attendee.PhotoName);

        if (bytes == null)
			throw new Exception("Not image to load");

		Photo = ImageSource.FromStream(() =>
		{
			return new MemoryStream(bytes);
		});
	}
	catch (Exception e)
	{
		LogHelper.Instance.AddLog(e);
	}
	finally
	{
		IsBusy = false;
	}
}
```

Congratulations! You finish hands on lab!

## And more... Some tips

* Listview Caching Strategy
* Pull to refresh in listview
* Menu context actions in listview
* Master detail pages / tabbed pages
