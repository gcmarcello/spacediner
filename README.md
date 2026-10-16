# Space Diner

Overcooked + Tower Defense num restaurante espacial. Sirva os aliens famintos
enquanto monstros, mafiosos e piratas espaciais tentam invadir a estacao.

**Engine:** Unity 2022.3 LTS (2D, Input Manager legado). Sem assets externos:
todo o mundo e gerado por codigo (`Assets/Scripts`).

## Como rodar
1. Unity Hub > Add > selecione esta pasta (aceite a versao 2022.3.x).
2. Abra qualquer cena (ou a cena vazia padrao) e aperte **Play**.
   `GameManager.Boot` cria camera, cozinha, torres e spawners sozinho.
3. Para gerar build: salve uma cena (File > Save As) e adicione em Build Settings.

## Controles
| Tecla | Acao |
|---|---|
| WASD / Setas | Mover |
| E / Espaco | Interagir (pegar, colocar, servir, construir) |
| Segurar E na TABUA | Cortar |
| R | Reiniciar apos game over |

## Loop
- **Carne**: CARNE > TABUA (segurar) > CHAPA (6s) > pegar a tempo (queima em +7s!) = Hamburguer
- **Verdura**: VERDURA > TABUA (segurar) = Salada
- Entregue o prato pedido no cliente do BALCAO (mais rapido = mais gorjeta). 6 clientes perdidos = game over.
- **Torres** (lado direito): construa por $60. Elas so atiram com **municao**: entregue
  Hamburguer (+6) ou Salada (+10) nelas. Maos vazias + dinheiro = melhorar (ate nv 3).
- Inimigos: **Monstro** (lento, tanque), **Pirata** (rapido, fraco), **Mafia** (ataca de longe).
  Se chegarem na parede, drenam a vida da estacao.
- Dinheiro vem de pratos servidos, inimigos mortos e bonus por onda.

## Ideias para evoluir
Sprites/audio reais, multiplayer local (co-op estilo Overcooked), mais receitas
(pratos montados), mais tipos de torre, chefes por onda, upgrades entre ondas.
