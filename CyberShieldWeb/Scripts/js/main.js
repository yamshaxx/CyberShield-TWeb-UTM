document.addEventListener('DOMContentLoaded', function () {
    // Navigation bar scroll behavior
    let lastScrollTop = 0
    const navbar = document.querySelector('.navbar')
    const scrollThreshold = 100

    // Create and insert placeholder
    const placeholder = document.createElement('div')
    placeholder.className = 'navbar-placeholder'
    navbar.parentNode.insertBefore(placeholder, navbar.nextSibling)

    let ticking = false
    let isAtBottom = false

    // Function to check if we're at the bottom of the page
    const checkIfAtBottom = () => {
        const windowHeight = window.innerHeight
        const documentHeight = Math.max(
            document.body.scrollHeight,
            document.body.offsetHeight,
            document.documentElement.clientHeight,
            document.documentElement.scrollHeight,
            document.documentElement.offsetHeight
        )
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop

        // Consider we're at the bottom if we're within 20px of the bottom
        return documentHeight - (scrollTop + windowHeight) < 20
    }

    // Handle scroll events with RAF for better performance
    window.addEventListener(
        'scroll',
        () => {
            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const currentScroll =
                        window.pageYOffset || document.documentElement.scrollTop

                    // Check if we're at the bottom of the page
                    isAtBottom = checkIfAtBottom()

                    // When at the bottom, keep the navbar hidden to prevent content jumping
                    if (currentScroll > scrollThreshold) {
                        if (currentScroll > lastScrollTop && !isAtBottom) {
                            // Scrolling down and not at bottom
                            navbar.classList.add('nav-hidden')
                            placeholder.classList.remove('show')
                        } else if (currentScroll < lastScrollTop) {
                            // Scrolling up
                            navbar.classList.remove('nav-hidden')
                            placeholder.classList.add('show')
                        }
                        // If at bottom and scrolling down, keep navbar hidden
                    } else {
                        // At the top
                        navbar.classList.remove('nav-hidden')
                        placeholder.classList.remove('show')
                    }

                    lastScrollTop = currentScroll <= 0 ? 0 : currentScroll
                    ticking = false
                })

                ticking = true
            }
        },
        { passive: true }
    )

    // Mobile menu toggle
    const mobileToggle = document.querySelector('.mobile-toggle')
    const navbarNav = document.querySelector('.navbar-nav')

    if (mobileToggle) {
        mobileToggle.addEventListener('click', () => {
            navbarNav.classList.toggle('show')
        })
    }

    // Close mobile menu when clicking outside
    document.addEventListener('click', e => {
        if (!navbar.contains(e.target) && navbarNav.classList.contains('show')) {
            navbarNav.classList.remove('show')
        }
    })

    // Smooth scrolling for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault()
            const target = document.querySelector(this.getAttribute('href'))
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                })
            }
        })
    })

    // Add some animation to elements when they come into view
    const animateOnScroll = () => {
        const elements = document.querySelectorAll(
            '.service-card, .benefit-card, .testimonial-card'
        )

        elements.forEach(element => {
            const elementTop = element.getBoundingClientRect().top
            const elementBottom = element.getBoundingClientRect().bottom

            if (elementTop < window.innerHeight && elementBottom > 0) {
                element.style.opacity = '1'
                element.style.transform = 'translateY(0)'
            }
        })
    }

    // Initial animation setup
    document
        .querySelectorAll('.service-card, .benefit-card, .testimonial-card')
        .forEach(element => {
            element.style.opacity = '0'
            element.style.transform = 'translateY(20px)'
            element.style.transition =
                'opacity 0.6s ease-out, transform 0.6s ease-out'
        })

    // Listen for scroll events
    window.addEventListener('scroll', animateOnScroll)
    // Initial check for elements in view
    window.addEventListener('load', animateOnScroll)
})
