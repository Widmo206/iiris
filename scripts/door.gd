extends Area2D

@export var level = ''
signal player_entered(level)


func _on_body_entered(_body):
	print('player entered door')
	player_entered.emit(level)
