extends Node2D

@export var level_name = '0'


# Called when the node enters the scene tree for the first time.
func _ready():
	$HUD.level(level_name)
	update_coin_counter()
	for coin in $Collectibles.get_children():
		coin.coin_collected.connect(_on_coin_collected)

func _on_door_player_entered(level):
	get_tree().change_scene_to_file.call_deferred(level)

func _on_coin_collected():
	update_coin_counter()

func update_coin_counter():
	$HUD.coins(Global.coins_collected)

func reset_level():
	get_tree().reload_current_scene.call_deferred()

func _input(event):
	if event.is_action_pressed('reset_level'):
		reset_level()
