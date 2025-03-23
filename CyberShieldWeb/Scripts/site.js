// Dropdown Menu Functionality
document.addEventListener('DOMContentLoaded', function () {
  const dropdowns = document.querySelectorAll('.navbar .dropdown')
  let timeoutId

  dropdowns.forEach(dropdown => {
    dropdown.addEventListener('mouseenter', function () {
      // La intrarea mouse-ului, anulăm orice timeout anterior
      if (timeoutId) {
        clearTimeout(timeoutId)
      }
      this.querySelector('.dropdown-menu').classList.add('show')
    })

    dropdown.addEventListener('mouseleave', function () {
      // La ieșirea mouse-ului, setăm un timeout de 1 secundă
      const menu = this.querySelector('.dropdown-menu')
      timeoutId = setTimeout(function () {
        menu.classList.remove('show')
      }, 700) // 1000ms = 1 secundă
    })
  })
})
