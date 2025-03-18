extends Node2D


func _ready():
	print_debug('MainMenu ready')
	$Options/StartButton.grab_focus()

func _process(_delta):
	pass

	if !OS.has_feature('pc'):
		$Options/FullscreenButton.hide()
		$Options/ExitButton.hide()

func _on_start_button_pressed():
	get_tree().change_scene_to_file("res://scenes/level_1.tscn")

func _on_fullscreen_button_pressed():
	if DisplayServer.window_get_mode() == DisplayServer.WINDOW_MODE_FULLSCREEN:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_WINDOWED)
	else:
		DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN)


func _on_exit_button_pressed():
	get_tree().quit()
