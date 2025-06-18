namespace riftofbuto;

using Godot;

public partial class MainMenu : Control
{
    // Button references
    private Button _playButton;
    private Button _upgradeButton;
    private Button _settingsButton;
    private Button _creditsButton;
    private Button _quitButton;
    
    // Audio (optional - add later if needed)
    private AudioStreamPlayer _bgMusic;
    private AudioStreamPlayer _buttonSfx;

    public override void _Ready()
    {
        // Get button references
        _playButton = GetNode<Button>("TitleContainer/MenuButtons/PlayButton");
        _upgradeButton = GetNode<Button>("TitleContainer/MenuButtons/UpgradeButton");
        _settingsButton = GetNode<Button>("TitleContainer/MenuButtons/SettingsButton");
        _creditsButton = GetNode<Button>("TitleContainer/MenuButtons/CreditsButton");
        _quitButton = GetNode<Button>("TitleContainer/MenuButtons/QuitButton");

        // Connect button signals
        _playButton.Pressed += OnPlayButtonPressed;
        _upgradeButton.Pressed += OnUpgradeButtonPressed;
        _settingsButton.Pressed += OnSettingsButtonPressed;
        _creditsButton.Pressed += OnCreditsButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressed;

        // Add hover effects
        SetupButtonHoverEffects();
        
        // Setup initial focus
        _playButton.GrabFocus();
        
        GD.Print("Main Menu initialized successfully");
    }

    private void SetupButtonHoverEffects()
    {
        // Add hover sound and visual feedback for each button
        Button[] buttons = { _playButton, _upgradeButton, _settingsButton, _creditsButton, _quitButton };
        
        foreach (Button button in buttons)
        {
            // Set pivot point to center for symmetric scaling
            button.PivotOffset = button.Size / 2;
            
            button.MouseEntered += () => OnButtonHover(button);
            button.MouseExited += () => OnButtonExit(button);
            button.FocusEntered += () => OnButtonFocus(button);
            button.FocusExited += () => OnButtonUnfocus(button);
        }
    }

    private void OnButtonHover(Button button)
    {
        // Play hover sound effect (implement when audio is added)
        // _buttonSfx?.Play();
        
        // Update pivot offset in case button size changed
        button.PivotOffset = button.Size / 2;
        
        // Add visual feedback
        button.Modulate = Colors.LightGreen;
        
        // Create subtle scaling effect
        var tween = CreateTween();
        tween.TweenProperty(button, "scale", Vector2.One * 1.05f, 0.1f);
        tween.TweenProperty(button, "modulate", Colors.LightGreen, 0.1f);
    }

    private void OnButtonExit(Button button)
    {
        // Reset button to normal state
        var tween = CreateTween();
        tween.Parallel().TweenProperty(button, "scale", Vector2.One, 0.1f);
        tween.Parallel().TweenProperty(button, "modulate", Colors.White, 0.1f);
    }

    private void OnButtonFocus(Button button)
    {
        // Update pivot offset
        button.PivotOffset = button.Size / 2;
        
        // Add visual feedback for focus (keyboard navigation)
        button.Modulate = Colors.LightBlue;
        
        var tween = CreateTween();
        tween.TweenProperty(button, "scale", Vector2.One * 1.03f, 0.1f);
    }

    private void OnButtonUnfocus(Button button)
    {
        // Reset button when focus is lost
        var tween = CreateTween();
        tween.Parallel().TweenProperty(button, "scale", Vector2.One, 0.1f);
        tween.Parallel().TweenProperty(button, "modulate", Colors.White, 0.1f);
    }

    // Button press handlers
    private void OnPlayButtonPressed()
    {
        GD.Print("Play Game pressed - Loading World.tscn");
        PlayButtonSound();
        
        // Transition to game world
        GetTree().ChangeSceneToFile("res://LevelSelection.tscn");
    }

    private void OnUpgradeButtonPressed()
    {
        GD.Print("Upgrade pressed - Opening upgrade menu");
        PlayButtonSound();
        
        // TODO: Implement upgrade menu
        GetTree().ChangeSceneToFile("res://UpgradeMenu.tscn");
    }

    private void OnSettingsButtonPressed()
    {
        GD.Print("Settings pressed - Opening settings menu");
        PlayButtonSound();
        
        // TODO: Implement settings menu
        ShowPlaceholderDialog("Settings", "Settings menu coming soon!\n\nOptions will include:\n• Audio Volume\n• Graphics Quality\n• Controls\n• Display Settings");
    }

    private void OnCreditsButtonPressed()
    {
        GD.Print("Credits pressed - Opening credits");
        PlayButtonSound();
        
        ShowCreditsDialog();
    }

    private void OnQuitButtonPressed()
    {
        GD.Print("Quit pressed - Exiting game");
        PlayButtonSound();
        
        // Show confirmation dialog
        ShowQuitConfirmation();
    }

    private void PlayButtonSound()
    {
        // TODO: Implement button click sound
        // _buttonSfx?.Play();
    }

    private void ShowPlaceholderDialog(string title, string message)
    {
        AcceptDialog dialog = new AcceptDialog();
        dialog.Title = title;
        dialog.DialogText = message;
        dialog.Size = new Vector2I(400, 300);
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        // Auto-remove dialog when closed
        dialog.Confirmed += () => dialog.QueueFree();
    }

    private void ShowCreditsDialog()
    {
        string creditsText = @"RIFT OF THE BUTO
        
Developed by:
Amr Fadhilah Abiyyu Alif Basysyar

Game Design Document ID: 231524002

Special Thanks:
• Indonesian Mythology for Buto Ijo inspiration
• Godot Engine Community

© 2025 - Made with Godot Engine";

        ShowPlaceholderDialog("Credits", creditsText);
    }

    private void ShowQuitConfirmation()
    {
        ConfirmationDialog confirmDialog = new ConfirmationDialog();
        confirmDialog.DialogText = "Are you sure you want to quit Rift of the Buto?";
        confirmDialog.Title = "Quit Game";
        
        AddChild(confirmDialog);
        confirmDialog.PopupCentered();
        
        // Handle confirmation
        confirmDialog.Confirmed += () => {
            GD.Print("Game quit confirmed");
            GetTree().Quit();
        };
        
        // Auto-remove dialog when closed
        confirmDialog.TreeExited += () => confirmDialog.QueueFree();
    }

    public override void _Input(InputEvent @event)
    {
        // Handle keyboard navigation
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Keycode)
            {
                case Key.Escape:
                    OnQuitButtonPressed();
                    break;
                case Key.Enter:
                    // Press the currently focused button
                    if (GetViewport().GuiGetFocusOwner() is Button focusedButton)
                    {
                        focusedButton.EmitSignal(Button.SignalName.Pressed);
                    }
                    break;
            }
        }
    }

    // Method for scene transitions with fade effect (optional)
    private void TransitionToScene(string scenePath)
    {
        // TODO: Add fade transition effect
        GetTree().ChangeSceneToFile(scenePath);
    }

    // Method called when returning to main menu from other scenes
    public void OnReturnToMenu()
    {
        GD.Print("Returned to main menu");
        _playButton.GrabFocus();
    }
}