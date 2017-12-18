using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PravoAdder.Domain;
using PravoAdder.Helpers;

namespace PravoAdder
{
	public class GuiEngine : IDisposable
	{
		private static Form _form;
		private static Settings _settings;
		private static InstanceEnviroment _instanceEnviroment;

		private const int WidthOffset = 20;
		private const int LineHeight = 25;
		private const int ColumnWidth = 150;
		private const int MaxLineCount = 10;

		public GuiEngine(string instanceEnviromentPath)
		{
			_instanceEnviroment = InstanceEnviroment.Read(instanceEnviromentPath);

			_settings = _instanceEnviroment.CurrentInstance.IsEmpty
				? new Settings()
				: new Settings(_instanceEnviroment.CurrentInstance.Name);
			
			_form = new Form();
		}

		public void SetGui(Settings settings)
		{
			var properties = typeof(Settings).GetProperties();
			foreach (Control control in _form.Controls)
			{
				if (string.IsNullOrEmpty(control.Name)) continue;
			
				var property = properties.First(p => p.Name.Equals(control.Name));
				var defaultValue = property.GetAttribute<DefaultValueAttribute>();

				if (control.GetType() == typeof(TextBox))
				{
					control.Text = defaultValue?.DefaultValue.ToString() ?? property.GetValue(settings)?.ToString();
				}

				if (control.GetType() == typeof(CheckBox))
				{
					((CheckBox) control).Checked = (bool) property.GetValue(settings);
				}

				if (control.GetType() == typeof(DateTimePicker))
				{
					var value = (DateTime) property.GetValue(settings);
					if (value == DateTime.MinValue) value = DateTime.Today;

					((DateTimePicker) control).Value = value;
				}

				if (control.GetType() == typeof(ComboBox))
				{
					var comboBox = (ComboBox) control;
					var value = property.GetValue(settings).ToString();
					var index = comboBox.FindStringExact(value);
					if (index >= 0)
					{
						
						comboBox.SelectedIndex = index;
					}				
				}
			}	
		}

		public void StartGui()
		{
			var lineCount = 0;
			var columnCount = 0;

			var controls = new List<Control>();

			foreach (var property in typeof(Settings).GetProperties())
			{
				var topPosition = lineCount * LineHeight;
				var control = new Control();

				if (property.PropertyType == typeof(string) || property.PropertyType == typeof(int))
				{
					control = new TextBox
					{
						Width = ColumnWidth,
						Left = ColumnWidth + 2 * columnCount * ColumnWidth,
						Top = topPosition,
						Name = property.Name,
					};
				}

				if (property.PropertyType == typeof(bool))
				{
					control = new CheckBox
					{
						Left = ColumnWidth + 2 * columnCount * ColumnWidth,
						Top = topPosition,
						Name = property.Name
					};
				}			

				if (property.PropertyType == typeof(DateTime))
				{
					control = new DateTimePicker
					{
						Left = ColumnWidth + 2 * columnCount * ColumnWidth,
						Top = topPosition,
						Name = property.Name,
						Width = ColumnWidth
					};
				}

				if (property.Name == "Name")
				{
					var actionsList = _instanceEnviroment.Instances
						.Select((x, count) => (x.Name, count))
						.ToDictionary(key => key.Name, value => value.count);

					var comboBox = CreateComboBox(columnCount, topPosition, actionsList, property.Name);
					comboBox.SelectedIndexChanged += (sender, args) =>
					{
						var cb = (ComboBox) sender;
						var instanceName = ((KeyValuePair<string, int>) cb.Items[cb.SelectedIndex]).Key;
						SetGui(new Settings(instanceName));
					};
					control = comboBox;
				}

				if (property.Name == typeof(ReadingMode).Name)
				{
					var actionsList = Enum.GetValues(property.PropertyType)
						.OfType<object>()
						.ToDictionary(key => Enum.GetName(property.PropertyType, key), value => (int) value);

					control = CreateComboBox(columnCount, topPosition, actionsList, property.Name);
				}

				if (property.Name == typeof(ProcessType).Name)
				{
					var actionsList = ProcessTypes.Properties
						.Select((value, count) => new KeyValuePair<string, int>(value.Name, count))
						.ToDictionary(x => x.Key, y => y.Value);

					control = CreateComboBox(columnCount, topPosition, actionsList, property.Name);
				}

				var labelText = property.Name;
				if (property.GetAttribute<RequiredAttribute>() != null) labelText += " (*)";
				var label = new Label
				{
					Width = ColumnWidth - WidthOffset,
					Text = labelText,
					Left = 2 * columnCount * ColumnWidth + WidthOffset,
					Top = topPosition
				};

				controls.Add(label);
				controls.Add(control);

				lineCount++;
				if (lineCount == MaxLineCount)
				{
					lineCount = 0;
					columnCount += 1;
				}
			}			

			InitForm(controls, lineCount, columnCount);
			SetGui(new Settings(_instanceEnviroment.CurrentInstance.Name));
			_form.ShowDialog();
		}

		private static void InitForm(IEnumerable<Control> controls, int lineCount, int columnCount)
		{
			var inputsHeight = columnCount > 0 ? MaxLineCount * LineHeight : lineCount * LineHeight;

			var startButton = new Button
			{
				Text = @"Start",
				Left = WidthOffset,
				Top = inputsHeight,
				Width = ColumnWidth - WidthOffset,
				FlatStyle = FlatStyle.Flat
			};
			startButton.MouseClick += StartButton_mouseClick;
			_form.Controls.Add(startButton);

			var saveButton = new Button
			{
				Text = @"Save",
				Left = ColumnWidth + WidthOffset,
				Top = inputsHeight,
				Width = ColumnWidth - WidthOffset,
				FlatStyle = FlatStyle.Flat
			};
			saveButton.MouseClick += SaveButton_mouseClick;
			_form.Controls.Add(saveButton);

			_form.Controls.AddRange(controls.ToArray());

			_form.Width = columnCount * ColumnWidth * 2 + WidthOffset * 2;
			_form.Height = inputsHeight + 75;
			_form.Text = @"Initializer";
			_form.FormBorderStyle = FormBorderStyle.FixedSingle;
			_form.BackColor = Color.White;
			_form.MaximizeBox = false;
		}

		private static ComboBox CreateComboBox(int columnCount, int topPosition, Dictionary<string, int> dataSource, string name)
		{
			var comboBox = new ComboBox
			{
				Width = ColumnWidth,
				Left = ColumnWidth + 2 * columnCount * ColumnWidth,
				Top = topPosition,
				DisplayMember = "Key",
				ValueMember = "Value",
				Name = name,
				BindingContext = _form.BindingContext
			};
			if (dataSource.Count != 0)
			{
				comboBox.DataSource = new BindingSource(dataSource, null);
			}

			return comboBox;
		}

		private static void SetSettings()
		{
			var settingsProperties = typeof(Settings).GetProperties();

			foreach (var control in _form.Controls)
			{
				if (control is CheckBox checkBox)
				{
					var property = settingsProperties
						.First(x => x.Name == checkBox.Name);
					property.SetValue(_settings, checkBox.Checked);
				}

				if (control is TextBox textBox)
				{
					var property = settingsProperties
						.First(x => x.Name == textBox.Name);
					var value = Convert.ChangeType(textBox.Text, property.PropertyType);
					property.SetValue(_settings, value);
				}

				if (control is ComboBox comboBox)
				{
					if (comboBox.Name == typeof(ProcessType).Name)
					{						
						if (comboBox.SelectedItem != null)
						{
							var property = settingsProperties
								.First(x => x.Name == comboBox.Name);
							var processName = ((KeyValuePair<string, int>) comboBox.SelectedItem).Key;
							property.SetValue(_settings, processName);
						}						
					}
					else if (comboBox.Name == "Name")
					{
						if (comboBox.Text != null)
						{
							var property = settingsProperties
								.First(x => x.Name == comboBox.Name);
							property.SetValue(_settings, comboBox.Text);
						}
					}
					else
					{
						var property = settingsProperties
							.First(x => x.Name == comboBox.Name);				
						var enumValue = Enum.ToObject(property.PropertyType, comboBox.SelectedIndex);
						property.SetValue(_settings, enumValue);
					}					
				}

				if (control is DateTimePicker dateTimePicker)
				{
					var property = settingsProperties
						.First(x => x.Name == dateTimePicker.Name);
					var value = Convert.ChangeType(dateTimePicker.Value, property.PropertyType);
					property.SetValue(_settings, value);
				}
			}

			_settings.SetFilePath();
			var instanceCreated = _settings.Save();

			var instance = new Instance { FileName = _settings.FilePath, Name = _settings.Name };
			_instanceEnviroment.CurrentInstance = instance;
			if (instanceCreated) _instanceEnviroment.Instances.Add(instance);
			_instanceEnviroment.Save();
		}

		private static void SaveButton_mouseClick(object sender, MouseEventArgs e)
		{
			SetSettings();
		}

		private static void StartButton_mouseClick(object sender, MouseEventArgs e)
		{
			SetSettings();
			_form.Dispose();

			new Engine(_settings).Run();
		}

		public void Dispose()
		{
			_form.Dispose();
		}
	}
}
