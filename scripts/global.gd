extends Node


var coins_collected = 0


# Called when the node enters the scene tree for the first time.
func _ready():
	print_debug('global ready')
	print_debug(coins_collected)

func _input(event):
	if event.is_action_pressed("return_to_main_menu"):
		get_tree().change_scene_to_file("res://main_menu.tscn")

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
